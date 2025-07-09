export interface IOrderItem {
  id?: string;
  orderId: string;
  bookId: string;
  quantity: number;
  priceAtPurchase: number;
}
