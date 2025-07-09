export enum UserRoles {
  Admin = 0,
  User = 1,
}

export enum OrderStatus {
  Pending = 0,
  Paid = 1,
  Shipped = 2,
  Delivered = 3,
  Cancelled = 4,
}

export enum PaymentStatus {
  Pending = 0,
  Completed = 1,
  Failed = 2,
  Refunded = 3,
  Cancelled = 4,
}
