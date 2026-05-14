import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import Login from './Login';
import { useAuth } from '../context/AuthContext';
import { BrowserRouter } from 'react-router-dom';

jest.mock('../context/AuthContext');

describe('Login Page', () => {
    beforeEach(() => {
        useAuth.mockReturnValue({
            login: jest.fn()
        });
    });

    test('renders login form', () => {
        render(
            <BrowserRouter>
                <Login />
            </BrowserRouter>
        );

        expect(screen.getByPlaceholderText(/Email/i)).toBeInTheDocument();
        expect(screen.getByPlaceholderText(/Password/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /Login/i })).toBeInTheDocument();
    });

    test('shows error message on failed login', async () => {
        window.alert = jest.fn();
        global.fetch = jest.fn().mockResolvedValue({
            ok: false,
            json: () => Promise.resolve({ message: 'Invalid credentials' })
        });

        render(
            <BrowserRouter>
                <Login />
            </BrowserRouter>
        );

        fireEvent.change(screen.getByPlaceholderText(/Email/i), { target: { value: 'test@test.com' } });
        fireEvent.change(screen.getByPlaceholderText(/Password/i), { target: { value: 'wrong' } });
        fireEvent.click(screen.getByRole('button', { name: /Login/i }));

        await waitFor(() => {
            expect(window.alert).toHaveBeenCalledWith('Invalid credentials');
        });
    });

});
