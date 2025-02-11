import { useState } from "react";
import { TokenResponse } from "../models/auth/jwt.respone";

export default function useToken() {

  function saveToken(token: TokenResponse) {
    localStorage.setItem('token', JSON.stringify(token));
    setToken(token);
  }

  function getToken(): TokenResponse | null {
    const tokenString = localStorage.getItem('token');
    if (tokenString) {
      return JSON.parse(tokenString);
    }
    return null;
  }

  const [token, setToken] = useState<TokenResponse | null>(getToken());

  return {
    setToken: saveToken,
    token
  }
}