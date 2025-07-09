import type { UserRoles } from '../utils/dictionary';

export interface IUser {
  id?: string;
  name: string;
  email: string;
  passwordHash: string;
  role: UserRoles;
}
