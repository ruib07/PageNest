import axios, { type AxiosRequestConfig } from 'axios';
import { RefreshToken } from '../authentications.service';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
const API_VERSION = import.meta.env.VITE_API_VERSION;

const apiRequest = async (
  method: 'GET' | 'POST' | 'PUT' | 'DELETE',
  endpoint: string,
  data?: any,
  withAuth: boolean = true
) => {
  const url = `${API_BASE_URL}/api/${API_VERSION}/${endpoint}`;

  const token = localStorage.getItem('token');
  const headers: AxiosRequestConfig['headers'] = {
    'Content-Type': 'application/json',
    ...(withAuth && token ? { Authorization: `Bearer ${token}` } : {}),
  };

  try {
    const response = await axios({ method, url, data, headers });

    return response;
  } catch (error: any) {
    console.error(`Error on request ${method} ${endpoint}:`, error);

    if (
      error.response?.status === 401 &&
      withAuth &&
      !endpoint.includes('auth/refresh') &&
      localStorage.getItem('refreshToken')
    ) {
      try {
        const newToken = await RefreshToken();

        const retryHeaders = {
          ...headers,
          Authorization: `Bearer ${newToken}`,
        };

        const retryResponse = await axios({
          method,
          url,
          data,
          headers: retryHeaders,
        });
        return retryResponse;
      } catch (refreshError) {
        console.error('Failed to renew token:', refreshError);

        window.location.href = '/authentication';
        throw refreshError;
      }
    }

    console.error(`Error on request ${method} ${endpoint}`);
    throw error;
  }
};

export default apiRequest;
