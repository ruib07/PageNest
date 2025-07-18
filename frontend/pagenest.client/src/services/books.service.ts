import type { IBook } from '../types/book';
import apiRequest from './helpers/api.service';

const route = 'books';

export const GetBooks = async () => apiRequest('GET', route, undefined, true);

export const GetBooksByAuthor = async (authorName: IBook['author']) =>
  apiRequest('GET', `${route}/author/${authorName}`, undefined, true);

export const GetBookById = async (bookId: IBook['id']) =>
  apiRequest('GET', `${route}/${bookId}`, undefined, true);

export const GetBookByTitle = async (bookTitle: IBook['title']) =>
  apiRequest('GET', `${route}/title/${bookTitle}`, undefined, true);

export const CreateBook = async (book: IBook) =>
  apiRequest('POST', route, book, true);

export const UpdateBook = async (
  bookId: IBook['id'],
  updateBook: Partial<IBook>
) => apiRequest('PUT', `${route}/${bookId}`, updateBook, true);

export const DeleteBook = async (bookId: IBook['id']) =>
  apiRequest('DELETE', `${route}/${bookId}`, undefined, true);
