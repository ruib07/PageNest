import type { OrderStatus } from '../utils/dictionary';

export interface IOrder {
  id?: string;
  userId: string;
  status: OrderStatus;
  total: number;
}

export interface IOrderRowProps {
  order: IOrder;
}
