import type { OrderStatus } from '../utils/dictionary';

export interface IOrder {
  id?: string;
  userId: string;
  status: OrderStatus;
  total: number;
}
