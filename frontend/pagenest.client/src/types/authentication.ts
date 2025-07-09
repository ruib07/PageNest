export interface ICreateUser {
  name: string;
  email: string;
  password: string;
  role: 1;
}

export interface ILoginCredentials {
  email: string;
  password: string;
}

export interface IDecodedToken {
  aud: string;
  iss: string;
  exp: number;
  iat: number;
  jti: string;
  nbf: number;
  id: string;
  role: string;
  sub: string;
  email: string;
}

export interface IRefreshToken {
  refreshToken: string;
}

export interface IAuthContextType {
  user: {
    id: string;
    role: number;
    token: string;
    name?: string;
    email?: string;
  } | null;
  login: (userData: {
    id: string;
    name: string;
    email: string;
    role: number;
    token: string;
  }) => void;
  logout: () => void;
}

export interface IResetPassword {
  email: string;
}

export interface IChangePassword {
  token: string;
  newPassword: string;
  confirmNewPassword: string;
}
