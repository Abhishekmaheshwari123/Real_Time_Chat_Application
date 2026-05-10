import React, { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(JSON.parse(localStorage.getItem("connecthub_user")));
    const [token, setToken] = useState(localStorage.getItem("connecthub_token"));

    const login = (userData, tokenData) => {
        localStorage.setItem("connecthub_user", JSON.stringify(userData));
        localStorage.setItem("connecthub_token", tokenData);
        setUser(userData);
        setToken(tokenData);
    };

    const logout = () => {
        localStorage.clear();
        setUser(null);
        setToken(null);
    };

    return (
        <AuthContext.Provider value={{ user, token, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
