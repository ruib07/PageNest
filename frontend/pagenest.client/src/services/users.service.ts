import type { IUser } from '../types/user';
import apiRequest from './helpers/api.service';

const route = 'users';

export const GetUsers = async () => apiRequest('GET', route, undefined, true);

export const GetUserById = async (userId: IUser['id']) =>
  apiRequest('GET', `${route}/${userId}`, undefined, true);

export const UpdateUser = async (
  userId: IUser['id'],
  updateUser: Partial<IUser>
) => apiRequest('PUT', `${route}/${userId}`, updateUser, true);

export const DeleteUser = async (userId: IUser['id']) =>
  apiRequest('DELETE', `${route}/${userId}`, undefined, true);
