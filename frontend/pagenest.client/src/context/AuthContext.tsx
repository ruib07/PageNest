import {
  createContext,
  type ReactNode,
  useContext,
  useEffect,
  useState,
} from 'react';
import type { IAuthContextType } from '../types/authentication';

const AuthContext = createContext<IAuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<any>(null);

  useEffect(() => {
    const fetchUser = () => {
      const id = localStorage.getItem('userId');
      const name = localStorage.getItem('name');
      const email = localStorage.getItem('email');
      const role = localStorage.getItem('role');
      const token = localStorage.getItem('token');

      if (id && name && email && role && token) {
        setUser({
          id,
          name,
          email,
          role: Number(role),
          token,
        });
      } else {
        logout();
      }
    };

    fetchUser();
  }, []);

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('name');
    localStorage.removeItem('email');
    localStorage.removeItem('role');
    localStorage.removeItem('refreshToken');
    setUser(null);
  };

  const login = (userData: {
    id: string;
    name: string;
    email: string;
    role: number;
    token: string;
  }) => {
    setUser(userData);
    localStorage.setItem('userId', userData.id);
    localStorage.setItem('name', userData.name);
    localStorage.setItem('email', userData.email);
    localStorage.setItem('role', String(userData.role));
    localStorage.setItem('token', userData.token);
  };

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = (): IAuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
