import mockUser from '../data/mockuser.json';
import type { AuthCredentials, AuthProvider } from './authentication';

export const mockAuthProvider: AuthProvider = {
  async signIn(credentials: AuthCredentials) {
    const isValidUser = credentials.email.toLowerCase() === mockUser.email.toLowerCase()
      && credentials.password === mockUser.password;

    if (!isValidUser) return null;

    return {
      id: mockUser.id,
      name: mockUser.name,
      email: mockUser.email,
    };
  },
};