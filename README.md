# ConnectHub - Backend

Welcome to the backend of **ConnectHub**, a real-time chat application. This repository contains the server-side microservices, providing the core logic and real-time communication capabilities.

## 🚀 Technologies Used
- **.NET 8**: Modern, high-performance cross-platform framework.
- **SignalR**: Real-time web functionality for instant messaging.
- **Entity Framework Core**: Efficient database management and ORM.
- **Microservices Architecture**:
  - **AuthService**: Handles user authentication and identity.
  - **ChatService**: Manages real-time messaging and conversation history.
  - **NotificationService**: Handles alerts and system notifications.

## 🛠️ Getting Started
1. Clone the repository.
2. Open `ConnectHub.sln` in your preferred IDE (Visual Studio, VS Code, or JetBrains Rider).
3. Update `appsettings.json` with your database connection strings.
4. Run the services using:
   ```bash
   dotnet run --project src/Service/AuthService/ConnectHub.Auth.API
   dotnet run --project src/Service/ChatService/ConnectHub.Chat.API
   ```

## 🔗 Frontend Repository
The frontend for this project can be found at [ConnectHub-Frontend](https://github.com/Abhishekmaheshwari123/ConnectHub-Frontend).
