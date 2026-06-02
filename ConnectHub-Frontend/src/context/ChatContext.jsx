import React, { createContext, useContext, useEffect, useState, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './AuthContext';

const ChatContext = createContext();
const API_GATEWAY_URL = 'http://localhost:7000';
const MEDIA_UPLOAD_ENDPOINTS = [
    `${API_GATEWAY_URL}/api/media/upload`
];

export const ChatProvider = ({ children }) => {
    const { token, user } = useAuth();
    const [connection, setConnection] = useState(null);
    const [conversations, setConversations] = useState([]);
    const [messages, setMessages] = useState([]);
    const [activePartner, setActivePartner] = useState(null);
    const [unreadCounts, setUnreadCounts] = useState({});

    const inferMediaType = (file) => {
        const mimeType = file?.type?.toLowerCase() || '';
        const fileName = file?.name?.toLowerCase() || '';

        if (mimeType.startsWith('image/')) return 'image';
        if (mimeType.startsWith('video/')) return 'video';

        if (
            mimeType === 'application/pdf' ||
            mimeType === 'application/msword' ||
            mimeType === 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' ||
            mimeType === 'application/vnd.ms-excel' ||
            mimeType === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' ||
            mimeType === 'application/vnd.ms-powerpoint' ||
            mimeType === 'application/vnd.openxmlformats-officedocument.presentationml.presentation' ||
            fileName.endsWith('.pdf') ||
            fileName.endsWith('.doc') ||
            fileName.endsWith('.docx') ||
            fileName.endsWith('.xls') ||
            fileName.endsWith('.xlsx') ||
            fileName.endsWith('.ppt') ||
            fileName.endsWith('.pptx') ||
            fileName.endsWith('.txt')
        ) {
            return 'document';
        }

        return 'file';
    };
    
    // Use a ref for activePartner so listeners can see the latest value without re-binding
    const activePartnerRef = useRef(activePartner);
    useEffect(() => { activePartnerRef.current = activePartner; }, [activePartner]);

    // 1. Initialize Connection
    useEffect(() => {
        if (token) {
            console.debug("SignalR: creating connection — token present:", !!token);
            try {
                // quick non-sensitive preview for debugging (first 10 chars)
                console.debug("SignalR: tokenPreview:", token ? `${token.slice(0, 10)}...` : null);
            } catch {}
            const newConnection = new signalR.HubConnectionBuilder()
                .withUrl(`${API_GATEWAY_URL}/chatHub`, { accessTokenFactory: () => token })
                .withAutomaticReconnect()
                .build();
            setConnection(newConnection);
        } else {
            setConnection(null);
        }
    }, [token]);

    // 2. Start Connection and Setup Listeners
    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => {
                    console.log("Connected to SignalR");
                    fetchConversations();
                    fetchUnread();
                })
                .catch(err => {
                    console.error("SignalR Connection Error: ", err);
                    try {
                        // provide a clearer console hint if negotiation fails with 401
                        if (err && err.toString && err.toString().includes('401')) {
                            console.warn('SignalR negotiation returned 401. Check that the client token is present and that the gateway forwards the token (query or header) to the Chat API.');
                        }
                    } catch {}
                });

            connection.onclose((error) => {
                console.warn('SignalR connection closed', error);
            });

            connection.on("ReceiveMessage", (data) => {
                const sender = (data.sender || data.Sender).toLowerCase();
                const receiver = (data.receiver || data.Receiver).toLowerCase();
                const myEmail = user?.email?.toLowerCase();
                const partner = (sender === myEmail) ? receiver : sender;

                if (partner === activePartnerRef.current?.toLowerCase()) {
                    setMessages(prev => {
                        const exists = prev.some(m => (m.id || m.Id) === (data.id || data.Id));
                        if (exists) return prev;
                        return [...prev, data];
                    });
                    if (sender !== myEmail) {
                        connection.invoke("MarkAsSeen", sender);
                    }
                }
                fetchConversations();
            });

            connection.on("MessageDelivered", (data) => {
                const mid = data.id || data.Id;
                setMessages(prev => prev.map(m => (m.id || m.Id) === mid ? { ...m, status: "Delivered", Status: "Delivered" } : m));
            });

            connection.on("MessagesSeen", (ids) => {
                setMessages(prev => prev.map(m => ids.includes(m.id || m.Id) ? { ...m, status: "Seen", Status: "Seen" } : m));
            });

            connection.on("ReceiveNotification", (data) => {
                const from = (data.from || data.From).toLowerCase();
                if (activePartnerRef.current?.toLowerCase() !== from) {
                    setUnreadCounts(prev => ({
                        ...prev,
                        [from]: (prev[from] || 0) + 1
                    }));
                }
            });

            return () => { connection.stop(); };
        }
    }, [connection, user?.email]); // Removed activePartner from dependencies

    const fetchConversations = async () => {
        if (!token) return;
        try {
            const res = await fetch(`${API_GATEWAY_URL}/api/chat/conversations`, {
                headers: { "Authorization": `Bearer ${token}` }
            });
            const data = await res.json();
            setConversations(data.sort((a, b) => new Date(b.Time) - new Date(a.Time)));
        } catch (e) {}
    };

    const fetchUnread = async () => {
        if (!token) return;
        try {
            const res = await fetch(`${API_GATEWAY_URL}/api/chat/unread`, {
                headers: { "Authorization": `Bearer ${token}` }
            });
            const data = await res.json();
            const counts = {};
            data.forEach(u => {
                const email = (u.user || u.User).toLowerCase();
                counts[email] = u.count || u.Count;
            });
            setUnreadCounts(counts);
        } catch (e) {}
    };

    const loadHistory = useCallback(async (email) => {
        if (!token) return;
        const res = await fetch(`${API_GATEWAY_URL}/api/chat/history/${email}`, {
            headers: { Authorization: `Bearer ${token}` }
        });
        const data = await res.json();
        setMessages(data);
        if (connection) connection.invoke("MarkAsSeen", email);
        
        setUnreadCounts(prev => {
            const newCounts = { ...prev };
            delete newCounts[email.toLowerCase()];
            return newCounts;
        });
    }, [token, connection]);

    const sendMessage = async (content, mediaUrl = null, messageType = "text", recipientEmail = activePartner) => {
        if (connection && recipientEmail && (content.trim() || mediaUrl)) {
            await connection.invoke("SendMessage", recipientEmail, content, mediaUrl, messageType);
        }
    };

    const shareMedia = async ({ file, recipientEmail = activePartner, caption = '' }) => {
        if (!file || !recipientEmail) return null;

        const mediaUrl = await uploadFile(file);

        const messageType = inferMediaType(file);
        const messageText = messageType === 'document' ? (caption || file.name) : caption;

        await sendMessage(messageText, mediaUrl, messageType, recipientEmail);
        return { mediaUrl, messageType };
    };

    const uploadFile = async (file) => {
        if (!token) return null;
        const formData = new FormData();
        formData.append("file", file);

        let lastError = null;

        for (const endpoint of MEDIA_UPLOAD_ENDPOINTS) {
            try {
                const res = await fetch(endpoint, {
                    method: "POST",
                    headers: { "Authorization": `Bearer ${token}` },
                    body: formData
                });

                const responseText = await res.text();

                if (!res.ok) {
                    throw new Error(responseText || `Media upload failed with status ${res.status}`);
                }

                if (!responseText) {
                    throw new Error("Media upload returned an empty response.");
                }

                try {
                    const data = JSON.parse(responseText);
                    if (data?.url) return data.url;
                    throw new Error(`Unexpected media upload response: ${responseText}`);
                } catch {
                    throw new Error(`Unexpected media upload response: ${responseText}`);
                }
            } catch (error) {
                lastError = error;
            }
        }

        throw lastError || new Error("Media upload failed.");
    };

    return (
        <ChatContext.Provider value={{ 
            conversations, messages, activePartner, setActivePartner, 
            unreadCounts, loadHistory, sendMessage, uploadFile, shareMedia 
        }}>
            {children}
        </ChatContext.Provider>
    );
};

export const useChat = () => useContext(ChatContext);
