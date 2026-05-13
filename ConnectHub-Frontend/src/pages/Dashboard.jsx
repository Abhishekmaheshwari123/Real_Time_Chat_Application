import React, { useState, useEffect, useRef } from 'react';
import { useAuth } from '../context/AuthContext';
import { useChat } from '../context/ChatContext';
import { LogOut, Send, Paperclip, FileText, Download } from 'lucide-react';

const Dashboard = () => {
    const { logout, user } = useAuth();
    const { 
        conversations, messages, activePartner, setActivePartner, 
        unreadCounts, loadHistory, sendMessage, shareMedia 
    } = useChat();
    const [inputText, setInputText] = useState('');
    const [uploading, setUploading] = useState(false);
    const fileInputRef = useRef(null);
    const [searchTerm, setSearchTerm] = useState('');
    const chatEndRef = useRef(null);

    useEffect(() => {
        if (activePartner) {
            loadHistory(activePartner);
        }
    }, [activePartner, loadHistory]);

    useEffect(() => {
        chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    const handleSend = (e) => {
        e.preventDefault();
        if (inputText.trim()) {
            sendMessage(inputText);
            setInputText('');
        }
    };

    const handleFileChange = async (e) => {
        const file = e.target.files[0];
        if (!file) return;

        if (!activePartner) {
            alert("Select a user first, then choose a file to share.");
            e.target.value = null;
            return;
        }

        setUploading(true);
        try {
            await shareMedia({ file });
        } catch (error) {
            alert(error?.message || "Media upload failed.");
        } finally {
            setUploading(false);
            e.target.value = null; // Reset input
        }
    };

    const formatDateLabel = (dateStr) => {
        const d = new Date(dateStr);
        const now = new Date();
        if (d.toDateString() === now.toDateString()) return "Today";
        now.setDate(now.getDate() - 1);
        if (d.toDateString() === now.toDateString()) return "Yesterday";
        return d.toLocaleDateString();
    };

    let lastDate = "";

    return (
        <div className="app-container">
            {/* SIDEBAR */}
            <div className="sidebar">
                <div className="sidebar-header">
                    <div className="user-profile">
                        <div className="avatar">{user?.email?.[0]?.toUpperCase() || '?'}</div>
                    </div>
                    <div className="actions">
                        <button onClick={logout} className="icon-btn"><LogOut size={20} /></button>
                    </div>
                </div>
                <div className="search-bar">
                    <input 
                        placeholder="Search by Email (Press Enter)" 
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        onKeyPress={(e) => {
                            if (e.key === 'Enter' && searchTerm) {
                                setActivePartner(searchTerm.toLowerCase().trim());
                                setSearchTerm(''); // Clear search after hitting enter
                            }
                        }}
                    />
                </div>
                <div className="chat-list">
                    {/* Show search result if not in conversations */}
                    {activePartner && !conversations.some(c => (c.User || c.user).toLowerCase() === activePartner.toLowerCase()) && (
                        <div className="chat-item active" onClick={() => setActivePartner(activePartner.toLowerCase())}>
                            <div className="avatar small">{activePartner[0]?.toUpperCase()}</div>
                            <div className="chat-item-info">
                                <span className="chat-item-name">{activePartner}</span>
                                <span className="chat-item-last-msg">New Conversation</span>
                            </div>
                        </div>
                    )}

                    {conversations.map(c => {
                        const partnerEmail = (c.User || c.user).toLowerCase();
                        const isActive = activePartner?.toLowerCase() === partnerEmail;
                        
                        return (
                            <div 
                                key={partnerEmail} 
                                className={`chat-item ${isActive ? 'active' : ''}`}
                                onClick={() => setActivePartner(partnerEmail)}
                            >
                                <div className="avatar small">{partnerEmail[0]?.toUpperCase() || '?'}</div>
                                <div className="chat-item-info">
                                    <span className="chat-item-name">{partnerEmail}</span>
                                    <span className="chat-item-last-msg">{c.LastMessage || c.lastMessage || 'No messages yet'}</span>
                                </div>
                                {unreadCounts[partnerEmail] > 0 && (
                                    <span className="badge">{unreadCounts[partnerEmail]}</span>
                                )}
                            </div>
                        );
                    })}
                </div>
            </div>

            {/* CHAT AREA */}
            <div className="chat-area">
                {activePartner ? (
                    <>
                        <div className="chat-header">
                            <div className="avatar small">{activePartner?.[0]?.toUpperCase() || '?'}</div>
                            <div style={{ marginLeft: '15px' }}>
                                <span style={{ fontWeight: 500 }}>{activePartner}</span>
                            </div>
                        </div>
                        <div className="messages-container">
                            {messages.map((m, i) => {
                                const currentDate = formatDateLabel(m.sentAt || m.SentAt);
                                const showDivider = lastDate !== currentDate;
                                lastDate = currentDate;
                                const messageType = (m.messageType || m.MessageType || '').toLowerCase();
                                const mediaUrl = m.mediaUrl || m.MediaUrl;
                                const fileLabel = m.message || m.content || m.Content || 'Download file';
                                
                                return (
                                        <React.Fragment key={m.id || i}>
                                        {showDivider && <div className="date-divider">{currentDate}</div>}
                                      <div className={`message ${( (m.sender || m.Sender)?.toLowerCase() === user?.email?.toLowerCase() ) ? 'sent' : 'received'}`}>
                                            {messageType === "image" ? (
                                                <img src={mediaUrl} alt="shared media" className="message-image" />
                                            ) : messageType === "video" ? (
                                                <video src={mediaUrl} controls className="message-video" />
                                            ) : (messageType === "document" || messageType === "file") ? (
                                                <div className="message-file">
                                                    <FileText size={20} />
                                                    <a href={mediaUrl} target="_blank" rel="noopener noreferrer">
                                                        {fileLabel}
                                                    </a>
                                                    <Download size={16} />
                                                </div>
                                            ) : null}
                                            
                                            {(messageType !== "file" && messageType !== "document") && (m.message || m.content || m.Content) && <div>{m.message || m.content || m.Content}</div>}
                                            
                                            <div className="message-footer">
                                                <span>{new Date(m.sentAt || m.SentAt || Date.now()).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                                                {( (m.sender || m.Sender)?.toLowerCase() === user?.email?.toLowerCase() ) && (
                                                    <span className={`tick ${((m.status || m.Status) || '').toLowerCase() === 'seen' ? 'seen' : ''}`}>
                                                        {((m.status || m.Status) || '').toLowerCase() === 'sent' ? '✓' : '✓✓'}
                                                    </span>
                                                )}
                                            </div>
                                        </div>
                                    </React.Fragment>
                                );
                            })}
                            <div ref={chatEndRef} />
                        </div>
                        <form className="input-container" onSubmit={handleSend}>
                            <input 
                                type="file" 
                                accept="image/*,video/*,.pdf,.doc,.docx,.txt,.xls,.xlsx,.ppt,.pptx"
                                style={{ display: 'none' }} 
                                ref={fileInputRef} 
                                onChange={handleFileChange} 
                            />
                            <button 
                                type="button" 
                                className="icon-btn" 
                                onClick={() => fileInputRef.current.click()}
                                disabled={uploading}
                            >
                                <Paperclip size={20} color="#54656f" />
                            </button>
                            <input 
                                placeholder={uploading ? "Uploading..." : "Type a message"} 
                                value={inputText}
                                onChange={(e) => setInputText(e.target.value)}
                                disabled={uploading}
                            />
                            <button type="submit" className="send-btn" disabled={uploading || (!inputText.trim())}>
                                <Send size={24} color={uploading || !inputText.trim() ? "#ccc" : "#54656f"} />
                            </button>
                        </form>
                    </>
                ) : (
                    <div className="empty-chat">
                        <div className="empty-chat-content">
                            <h2>ConnectHub</h2>
                            <p>Select a chat to start messaging</p>
                        </div>
                    </div>
                )}
            </div>

        </div>
    );
};

export default Dashboard;
