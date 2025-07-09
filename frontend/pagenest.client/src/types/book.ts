export interface IBook {
  id?: string;
  title: string;
  author: string;
  description: string;
  publishedDate: Date;
  isbn: string;
  pageCount: number;
  languageId: string;
  coverImageUrl: string;
  stock: number;
  price: number;
  categoryId: string;
}
