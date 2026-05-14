import React from 'react';
import { render, act } from '@testing-library/react';
import { ChatProvider, useChat } from './ChatContext';
import { useAuth } from './AuthContext';
import * as signalR from '@microsoft/signalr';

// Mock dependencies
jest.mock('./AuthContext');
jest.mock('@microsoft/signalr');

const TestComponent = ({ onChat }) => {
    const chat = useChat();
    React.useEffect(() => {
        if (chat) onChat(chat);
    }, [chat]);
    return null;
};

describe('ChatContext', () => {
    let mockAuth;

    beforeEach(() => {
        jest.clearAllMocks();
        mockAuth = {

            token: 'fake-token',
            user: { email: 'alice@test.com' }
        };
        useAuth.mockReturnValue(mockAuth);

        // Mock SignalR
        signalR.HubConnectionBuilder.mockReturnValue({
            withUrl: jest.fn().mockReturnThis(),
            withAutomaticReconnect: jest.fn().mockReturnThis(),
            build: jest.fn().mockReturnValue({
                start: jest.fn().mockResolvedValue(),
                on: jest.fn(),
                off: jest.fn(),
                stop: jest.fn(),
                invoke: jest.fn()
            })
        });
    });

    test('initializes signalR connection when token is present', () => {
        render(
            <ChatProvider>
                <TestComponent onChat={() => {}} />
            </ChatProvider>
        );

        expect(signalR.HubConnectionBuilder).toHaveBeenCalled();
    });

    test('does not initialize connection when token is missing', () => {
        useAuth.mockReturnValue({ token: null, user: null });
        render(
            <ChatProvider>
                <TestComponent onChat={() => {}} />
            </ChatProvider>
        );

        expect(signalR.HubConnectionBuilder).not.toHaveBeenCalled();
    });

    test('shareMedia calls uploadFile and sendMessage', async () => {
        let chatRef;
        render(
            <ChatProvider>
                <TestComponent onChat={(c) => { chatRef = c; }} />
            </ChatProvider>
        );

        // Mock fetch for upload
        global.fetch = jest.fn().mockResolvedValue({
            ok: true,
            text: () => Promise.resolve(JSON.stringify({ url: 'http://blob/file.png' }))
        });

        const file = new File(['foo'], 'photo.png', { type: 'image/png' });
        
        await act(async () => {
            await chatRef.shareMedia({ file, recipientEmail: 'bob@test.com' });
        });

        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/media/upload'),
            expect.objectContaining({ method: 'POST' })
        );
    });
});
