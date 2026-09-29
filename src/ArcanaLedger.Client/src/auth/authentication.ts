export interface AuthCredentials {
  email: string;
  password: string;
}

export interface AuthenticatedUser {
  id: string;
  name: string;
  email: string;
}

export interface AuthProvider {
  signIn(credentials: AuthCredentials): Promise<AuthenticatedUser | null>;
}