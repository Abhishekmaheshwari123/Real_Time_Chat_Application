# ConnectHub: System Design Document

This document outlines the **High-Level Design (HLD)** and **Low-Level Design (LLD)** for the **ConnectHub Chat Application**, a production-grade, real-time messaging microservices system. It includes architecture diagrams, detailed feature flowcharts, database ER diagrams, and UML class diagrams.

---

# 1. High-Level Design (HLD)

The High-Level Design defines the system structure, responsibilities of each component, and the real-time communication patterns.

```
+-------------------------------------------------------------------+
|                          React Frontend                           |
+-------------------------------------------------------------------+
                                  | HTTP / WebSockets
                                  v
+-------------------------------------------------------------------+
|                        Ocelot API Gateway                         |
+-------------------------------------------------------------------+
         |                     |                     |
         +-------------+       +-------------+       +-------------+
         | (Auth API)  |       | (Chat API)  |       | (Media API) |
         v             v             v             v             v
   +-----------+ +-----------+ +-----------+ +-----------+ +-----------+
   |  Auth     | |  Chat     | |  Media    | | Notification| | Gateway   |
   |  Service  | |  Service  | |  Service  | |  Service    | |  Service  |
   +-----------+ +-----------+ +-----------+ +-----------+ +-----------+
         |             |             |             |
         v             v             v             v
    [(Auth DB)]   [(Chat DB)]  [Azure Blob]   [(Notify DB)]
```

---

## 1.1. Core Architectural Feature Flowcharts

### A. Authentication & OAuth Flow Chart
This flowchart describes the path for both Traditional (Email/Password) Login/Register and Google OAuth.

```mermaid
sequenceDiagram
    autonumber
    actor User as Client (React App)
    participant Gateway as API Gateway
    participant Auth as Auth Microservice
    participant AuthDB as Auth Database
    participant Google as Google Identity Server

    alt Traditional Email/Password Login
        User->>Gateway: POST /api/auth/login (Email, Password)
        Gateway->>Auth: Forward Login Request
        Auth->>AuthDB: Query User by Email
        AuthDB-->>Auth: User Entity (PasswordHash)
        Auth->>Auth: Verify BCrypt / Argon2 Password Hash
        alt Credentials Invalid
            Auth-->>User: 401 Unauthorized
        else Credentials Valid
            Auth->>Auth: Generate JWT Token (Claims: UserId, Email, Role)
            Auth-->>User: 200 OK (JWT Token, User Profile)
        end
    else Google OAuth Login
        User->>Google: Prompt Login Screen & Authenticate
        Google-->>User: Return Google Authorization code/id_token
        User->>Gateway: POST /api/auth/google-login (id_token)
        Gateway->>Auth: Forward Google Login Request
        Auth->>Google: Verify token with Google API Client
        Google-->>Auth: Verified Google User Profile (Email, Name, GoogleId)
        Auth->>AuthDB: Query User by Email/GoogleId
        alt User Does Not Exist
            Auth->>AuthDB: Create New User Record (Set Flag: IsOAuthUser)
            AuthDB-->>Auth: Saved User Record
        end
        Auth->>Auth: Generate JWT Token (Claims: UserId, Email)
        Auth-->>User: 200 OK (JWT Token, User Profile)
    end
```

---

### B. Real-Time Chat & Seen Receipt Flow Chart
This sequence diagram shows how clients establish a WebSocket connection and how a chat message and read receipt ("Seen" ticks) are processed in real-time.

```mermaid
sequenceDiagram
    autonumber
    actor Alice as Client Alice (Sender)
    actor Bob as Client Bob (Receiver)
    participant Gateway as API Gateway
    participant ChatHub as Chat SignalR Hub
    participant ChatAPI as Chat Microservice
    participant ChatDB as Chat Database

    %% SignalR Connection Establishment %%
    Bob->>Gateway: Connect WebSocket /negotiate
    Gateway->>ChatHub: Establish Connection
    ChatHub->>ChatHub: Map Bob's ConnectionId to UserId (Bob)
    Note over Bob, ChatHub: WebSocket Connection Active

    %% Send Message Flow %%
    Alice->>Gateway: Send Message via WebSocket / HTTP
    Gateway->>ChatHub: OnSendMessage(ReceiverId, Content)
    ChatHub->>ChatAPI: Process & Validate Message
    ChatAPI->>ChatDB: Insert Message (Status: Sent, Timestamp: Now)
    ChatDB-->>ChatAPI: Saved Message (With Id)
    
    ChatHub->>Bob: Emit "ReceiveMessage" (Message DTO)
    Bob-->>ChatHub: Acknowledge delivery
    ChatHub->>ChatDB: Update Message Status (Status: Delivered)
    ChatHub->>Alice: Emit "MessageStatusUpdated" (Id, Delivered)

    %% Seen Receipt Flow %%
    Note over Bob: Bob opens chat window with Alice
    Bob->>Gateway: Send Seen Event (MessageId)
    Gateway->>ChatHub: OnMessageSeen(MessageId, SenderId)
    ChatHub->>ChatAPI: Mark message as read
    ChatAPI->>ChatDB: Update Message Status (Status: Read/Seen)
    ChatHub->>Alice: Emit "MessageSeen" (MessageId)
    Note over Alice: Alice UI updates to show blue double ticks
```

---

### C. Media Sharing & Persistent Upload Flow Chart
This flow details how rich media (images, files, PDFs) are shared and distributed securely.

```mermaid
sequenceDiagram
    autonumber
    actor Alice as Client Alice
    actor Bob as Client Bob
    participant Gateway as API Gateway
    participant Media as Media Microservice
    participant Blob as Azure Blob Storage
    participant Chat as Chat Microservice

    Alice->>Gateway: POST /api/media/upload (File Binary)
    Gateway->>Media: Forward Media Payload
    Media->>Media: Validate File Type & Max Size
    Media->>Blob: Upload File to Container
    Blob-->>Media: Return Permanent Blob URL
    Media->>Media: Generate Thumbnail (if image/video)
    Media-->>Alice: 200 OK (Blob URL, Thumbnail URL, FileType)
    
    Note over Alice: Alice attaches Media Metadata to chat message payload
    Alice->>Gateway: POST /api/chat/messages (Content: "", MediaUrl: BlobURL)
    Gateway->>Chat: Process Chat Message with Media
    Chat->>Chat: Save to ChatDB (MediaUrl referenced)
    Chat->>Bob: Real-Time SignalR Broadcast (ReceiveMessage)
    Note over Bob: Bob's client renders media viewer
```

---

### D. Offline Notifications System Flow Chart
This flow describes the actions taken when the receiver is offline and requires standard notifications.

```mermaid
sequenceDiagram
    autonumber
    actor Alice as Client Alice
    participant ChatHub as Chat SignalR Hub
    participant ChatAPI as Chat Microservice
    participant Notify as Notification Microservice
    participant NotifyDB as Notification DB
    actor Bob as Client Bob (Offline)

    Alice->>ChatHub: Send Message to Bob
    ChatHub->>ChatHub: Check Bob's active connections
    Note over ChatHub: Bob is not connected (Offline)
    ChatHub->>ChatAPI: Process message normally
    ChatAPI->>Notify: Raise Event: MessageUndelivered (MessageId, ReceiverId, Content)
    Notify->>NotifyDB: Save PushNotification Queue (Pending state)
    
    alt Push Notification Service Integration (e.g. Firebase)
        Notify->>NotifyDB: Poll Pending Notifications
        Notify->>Notify: Construct FCM Notification
        Notify->>Bob: Push FCM Payload (Mobile/Web Service Worker)
        Note over Bob: Mobile device receives Push Notification banner
    end
```

---

# 2. Low-Level Design (LLD)

The Low-Level Design defines the implementation details, class layouts, schemas, and structural constraints.

## 2.1. Structural UML Class Diagram (Clean Architecture Layering)
ConnectHub implements a modular **Clean Architecture**. This UML diagram illustrates the logical separation and relationship between layers in our services (e.g., the Chat microservice).

```mermaid
classDiagram
    %% Clean Architecture Layering %%
    
    %% Domain Layer (Center) %%
    class EntityBase {
        <<Abstract>>
        +Guid Id
        +DateTime CreatedAt
    }
    class Message {
        +Guid SenderId
        +Guid ReceiverId
        +string Content
        +string MediaUrl
        +DateTime Timestamp
        +MessageStatus Status
    }
    class Conversation {
        +Guid User1Id
        +Guid User2Id
        +Guid LastMessageId
        +DateTime UpdatedAt
    }
    EntityBase <|-- Message
    EntityBase <|-- Conversation

    %% Application Layer %%
    class IChatRepository {
        <<Interface>>
        +GetMessagesAsync(Guid conversationId) Task~IEnumerable~Message~~
        +SaveMessageAsync(Message msg) Task~bool~
        +GetConversationAsync(Guid user1, Guid user2) Task~Conversation~
    }
    class ISignalRService {
        <<Interface>>
        +SendMessageToUserAsync(Guid userId, MessageDto message) Task
        +SendSeenReceiptAsync(Guid senderId, Guid messageId) Task
    }
    class SendMessageCommandHandler {
        +IChatRepository _chatRepo
        +ISignalRService _signalRService
        +Handle(SendMessageCommand cmd) Task~MessageDto~
    }
    SendMessageCommandHandler ..> IChatRepository : Uses
    SendMessageCommandHandler ..> ISignalRService : Uses

    %% Infrastructure Layer %%
    class ChatDbContext {
        +DbSet~Message~ Messages
        +DbSet~Conversation~ Conversations
        +OnModelCreating(ModelBuilder mb)
    }
    class ChatRepository {
        +ChatDbContext _context
        +GetMessagesAsync(Guid conversationId)
    }
    class SignalRService {
        +IHubContext~ChatHub~ _hubContext
    }
    IChatRepository <|-- ChatRepository : Implements
    ISignalRService <|-- SignalRService : Implements
    ChatRepository --> ChatDbContext : Queries

    %% API Presentation Layer %%
    class ChatController {
        +IMediator _mediator
        +SendMessage(SendMessageRequest req) Task~IActionResult~
    }
    class ChatHub {
        +SendMessage(string receiverId, string content) Task
        +MarkAsRead(string messageId, string senderId) Task
        +OnConnectedAsync() Task
        +OnDisconnectedAsync(Exception ex) Task
    }
    ChatController --> SendMessageCommandHandler : Dispatches Command
    ChatHub --> SendMessageCommandHandler : Directly Invokes / Dispatches
```

---

## 2.2. Database Entity-Relationship (ER) Diagram
Below is the ER Diagram mapping the primary relational databases. Since we leverage a Microservices pattern, the schemas reside across independent databases (`AuthDB` and `ChatDB`) but are related logically via global `UserId` keys.

```mermaid
erDiagram
    %% AuthDB Schema %%
    USERS ||--o{ USER_ROLES : has
    USERS {
        Guid Id PK
        string Email UK
        string PasswordHash
        string DisplayName
        string ProfilePictureUrl
        string GoogleId NULL
        DateTime CreatedAt
    }
    USER_ROLES {
        Guid UserId FK
        string RoleName
    }

    %% ChatDB Schema %%
    CONVERSATIONS ||--o{ MESSAGES : contains
    CONVERSATIONS {
        Guid Id PK
        Guid User1Id
        Guid User2Id
        Guid LastMessageId FK
        DateTime UpdatedAt
    }
    MESSAGES {
        Guid Id PK
        Guid ConversationId FK
        Guid SenderId
        Guid ReceiverId
        string Content
        string MediaUrl NULL
        DateTime Timestamp
        string Status "Sent | Delivered | Read"
    }

    %% NotificationDB Schema %%
    NOTIFICATIONS {
        Guid Id PK
        Guid RecipientId
        string Title
        string Body
        bool IsRead
        DateTime CreatedAt
    }
```

---

## 2.3. Design Patterns Applied

### 1. CQRS (Command Query Responsibility Segregation)
By segregating write operations (Commands) from read operations (Queries), performance, scalability, and security are optimized.
- **Example Command:** `SendMessageCommand` -> Handled by changing DB state and publishing SignalR updates.
- **Example Query:** `GetChatHistoryQuery` -> Bypasses heavy business rules to pull directly from read-optimized DbContext mappings.

### 2. Dependency Injection & Repository Patterns
- All external calls (database queries, network requests, Azure operations) are isolated behind interfaces (`IChatRepository`, `IBlobStorageService`).
- ASP.NET DI handles standard service lifetimes (`Scoped` for repositories and contexts, `Singleton` for persistent helper utilities, `Transient` for transient request-level commands).

### 3. Gateway Routing & Reverse Proxy (Ocelot)
- ConnectHub Gateway exposes unified endpoints to the frontend, transforming external paths (e.g. `/api/v1/chat/messages`) internally to private addresses (e.g. `http://chat-api:8080/messages`) transparently.

---

## 2.4. Detailed Low-Level Component Responsibilities

| Service | Primary Component Class | Method Name | Functionality |
| :--- | :--- | :--- | :--- |
| **Auth** | `AuthService` | `GenerateJwtToken(User user)` | Builds token with `Claims` (Id, Role, Name) signed using HS256 algorithm and configured environment secret. |
| | `AuthService` | `RegisterUser(RegisterDto dto)` | Hashes raw password using BCrypt and saves user to Auth database. |
| **Chat** | `ChatHub` | `OnConnectedAsync()` | Reads authenticating JWT claim from query parameter, maps `Context.ConnectionId` to User ID using internal thread-safe memory storage. |
| | `ChatHub` | `SendMessage(string to, string msg)` | Broadcasts real-time events to dynamic Client connection IDs. |
| | `ChatRepository` | `GetConversationMessages(Guid convId)` | Returns historical messages sorted sequentially in chronological order. |
| **Media** | `BlobStorageService` | `UploadFileAsync(IFormFile file)` | Sanitizes file name, generates a unique GUID prefix, uploads stream directly to Azure Storage, and outputs a secure direct SAS URI. |
| **Notification** | `PushService` | `QueueNotification(Guid userId)` | Saves notification payloads to local DB to retry offline dispatches reliably. |
