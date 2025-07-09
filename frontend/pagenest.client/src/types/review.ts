export interface IReview {
  id?: string;
  userId: string;
  bookId: string;
  rating: number;
  comment: string;
}
