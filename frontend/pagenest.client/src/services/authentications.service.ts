import axios from 'axios';
import type {
  IChangePassword,
  ICreateUser,
  ILoginCredentials,
  IRefreshToken,
  IResetPassword,
} from '../types/authentication';
import apiRequest from './helpers/api.service';

const route = 'auth';

export const SignUp = async (newUser: ICreateUser) =>
  apiRequest('POST', `${route}/signup`, newUser, false);

export const Authentication = async (auth: ILoginCredentials) =>
  apiRequest('POST', `${route}/signin`, auth, false);

export const Logout = async (refreshToken: IRefreshToken['refreshToken']) =>
  apiRequest('POST', `${route}/logout`, { refreshToken }, true);

export const RefreshToken = async () => {
  const refreshToken = localStorage.getItem('refreshToken');
  if (!refreshToken) throw new Error('No refresh token available.');

  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
  const API_VERSION = import.meta.env.VITE_API_VERSION;

  const response = await axios.post(
    `${API_BASE_URL}/api/${API_VERSION}/auth/refresh`,
    {
      refreshToken,
    }
  );

  const { accessToken, refreshToken: newRefreshToken } = response.data;

  localStorage.setItem('token', accessToken);
  localStorage.setItem('refreshToken', newRefreshToken);

  return accessToken;
};

export const SendEmail = async (email: IResetPassword) =>
  apiRequest('POST', `${route}/recover-password`, email, false);

export const UpdatePassword = async (newPassword: IChangePassword) =>
  apiRequest('PUT', `${route}/update-password`, newPassword, false);
