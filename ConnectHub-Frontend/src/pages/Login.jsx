import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

const Login = () => {
    const [isRegister, setIsRegister] = useState(false);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [username, setUsername] = useState('');
    const { login } = useAuth();
    const navigate = useNavigate();

    // Replace this with your actual Google Client ID
    const GOOGLE_CLIENT_ID = "460935468037-rfmafkhgvbtqvnevecln34njkl0icl6m.apps.googleusercontent.com";
    const API_GATEWAY_URL = 'http://localhost:7000';

    useEffect(() => {
        /* global google */
        const initGoogle = () => {
            if (window.google) {
                google.accounts.id.initialize({
                    client_id: GOOGLE_CLIENT_ID,
                    callback: handleGoogleResponse
                });
                google.accounts.id.renderButton(
                    document.getElementById("googleBtn"),
                    { theme: "outline", size: "large", width: 350 }
                );
            }
        };

        // Small timeout to ensure script is loaded and DOM is ready
        const timer = setTimeout(initGoogle, 100);
        return () => clearTimeout(timer);
    }, [isRegister]);

    const handleGoogleResponse = async (response) => {
        try {
            const res = await fetch(`${API_GATEWAY_URL}/api/auth/google`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ credential: response.credential })
            });
            const data = await res.json();
            if (data.token) {
                login(data.user, data.token);
                navigate('/');
            } else {
                alert(data.message || "Google login failed");
            }
        } catch (err) {
            console.error(err);
            alert("Auth Service is Sleeping");
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const endpoint = isRegister ? 'register' : 'login';
        const body = isRegister ? { userName: username, email, password } : { email, password };

        try {
            const res = await fetch(`${API_GATEWAY_URL}/api/auth/${endpoint}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            const data = await res.json();

            if (isRegister) {
                if (res.ok) {
                    alert("Registration successful! Please login.");
                    setIsRegister(false);
                } else {
                    alert(data.message || "Registration failed");
                }
            } else {
                if (data.token) {
                    const userData = data.user || { email };
                    login(userData, data.token);
                    navigate('/');
                } else {
                    alert(data.message || "Login failed");
                }
            }
        } catch (err) {
            console.error(err);
            alert("Server not reachable");
        }
    };

    return (
        <div className="login-container">
            <div className="login-card">
                <h2>ConnectHub</h2>
                <p className="subtitle">{isRegister ? 'Create your account' : 'Welcome back!'}</p>
                
                <form onSubmit={handleSubmit}>
                    {isRegister && (
                        <input 
                            type="text" 
                            placeholder="Username" 
                            value={username} 
                            onChange={(e) => setUsername(e.target.value)} 
                            required 
                        />
                    )}
                    <input 
                        type="email" 
                        placeholder="Email" 
                        value={email} 
                        onChange={(e) => setEmail(e.target.value)} 
                        required 
                    />
                    <input 
                        type="password" 
                        placeholder="Password" 
                        value={password} 
                        onChange={(e) => setPassword(e.target.value)} 
                        required 
                    />
                    <button type="submit" className="main-btn">
                        {isRegister ? 'CREATE ACCOUNT' : 'LOGIN'}
                    </button>
                </form>

                <div className="divider">
                    <span>OR</span>
                </div>

                {/* Google Sign-In Button Container */}
                <div id="googleBtn" style={{ width: '100%', display: 'flex', justifyContent: 'center' }}></div>

                <p className="toggle-text">
                    {isRegister ? 'Already have an account?' : "Don't have an account?"}{' '}
                    <span onClick={() => setIsRegister(!isRegister)}>
                        {isRegister ? 'Login' : 'Create one'}
                    </span>
                </p>
            </div>
        </div>
    );
};

export default Login;
