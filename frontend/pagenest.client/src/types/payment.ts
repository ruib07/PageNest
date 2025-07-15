import type { PaymentStatus } from '../utils/dictionary';

export interface IPayment {
  id?: string;
  orderId: string;
  amount: number | string;
  stripePaymentIntentId: string;
    status: PaymentStatus;
    createdAt?: Date;
}

export interface IPaymentRowProps {
    payment: IPayment;
}
