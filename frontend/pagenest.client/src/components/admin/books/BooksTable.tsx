import { useEffect, useState } from 'react';
import { useDeleteBook } from '../../../hooks/admin/books/useDeleteBook';
import { useEditBook } from '../../../hooks/admin/books/useEditBook';
import { useModal } from '../../../hooks/useModal';
import { ChevronLeftIcon, ChevronRightIcon } from '../../../icons';
import { GetBooks } from '../../../services/books.service';
import { GetCategoryById } from '../../../services/categories.service';
import { GetLanguageById } from '../../../services/languages.service';
import type { IBook } from '../../../types/book';
import type { ICategory } from '../../../types/category';
import type { ILanguage } from '../../../types/language';
import {
  Table,
  TableBody,
  TableCell,
  TableHeader,
  TableRow,
} from '../../ui/Table';
import BookRow from './BookRow';
import DeleteBookModal from './DeleteBookModal';
import EditBookModal from './EditBookModal';

export default function BooksTable() {
  const [books, setBooks] = useState<IBook[]>([]);
  const [languages, setLanguages] = useState<{ [key: string]: ILanguage }>({});
  const [categories, setCategories] = useState<{ [key: string]: ICategory }>(
    {}
  );
  const [, setError] = useState<string | null>(null);
  const { isOpen, modalId, modalType, openModal, closeModal } = useModal();
  const { deleteBook } = useDeleteBook({
    books,
    setBooks,
    closeModal,
    setError,
  });
  const { editBook } = useEditBook({
    books,
    setBooks,
    closeModal,
    setError,
  });
  const [editedBook, setEditedBook] = useState<Partial<IBook>>({
    title: '',
    author: '',
    description: '',
    publishedDate: undefined,
    isbn: '',
    pageCount: undefined,
    languageId: '',
    coverImageUrl: '',
    stock: undefined,
    price: undefined,
    categoryId: '',
  });
  const [currentPage, setCurrentPage] = useState(1);
  const booksPerPage = 10;

  useEffect(() => {
    const fetchBooks = async () => {
      try {
        const booksResponse = await GetBooks();
        const booksData = booksResponse.data;
        setBooks(booksData);

        const languagesMap: { [key: string]: ILanguage } = {};
        const categoriesMap: { [key: string]: ICategory } = {};

        await Promise.all(
          booksData.map(async (book: IBook) => {
            if (book.languageId && !languagesMap[book.languageId]) {
              const languageResponse = await GetLanguageById(book.languageId);
              languagesMap[book.languageId] = languageResponse.data;
            }
            if (book.categoryId && !categoriesMap[book.categoryId]) {
              const categoryResponse = await GetCategoryById(book.categoryId);
              categoriesMap[book.categoryId] = categoryResponse.data;
            }
          })
        );

        setLanguages(languagesMap);
        setCategories(categoriesMap);
      } catch {
        setError('Failed to fetch books.');
      }
    };

    fetchBooks();
  }, []);

  const handleOpenEditModal = (book: IBook) => {
    setEditedBook({
      title: book.title,
      author: book.author,
      description: book.description,
      publishedDate: book.publishedDate,
      isbn: book.isbn,
      pageCount: book.pageCount,
      languageId: book.languageId,
      coverImageUrl: book.coverImageUrl,
      stock: book.stock,
      price: book.price,
      categoryId: book.categoryId,
    });
    openModal(book.id, 'edit');
  };

  const indexOfLastBook = currentPage * booksPerPage;
  const indexOfFirstBook = indexOfLastBook - booksPerPage;

  const totalPages = Math.ceil(books.length / booksPerPage);

  const goToPage = (page: number) => {
    if (page >= 1 && page <= totalPages) setCurrentPage(page);
  };

  return (
    <div className="overflow-hidden rounded-xl border border-gray-200 bg-white dark:border-white/[0.05] dark:bg-white/[0.03]">
      <div className="max-w-full overflow-x-auto">
        <div className="min-w-[1102px]">
          <Table>
            <TableHeader className="border-b border-gray-100 dark:border-white/[0.05]">
              <TableRow>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Cover Image
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Title
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Author
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Description
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Published Date
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  ISBN
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Page Count
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Language
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Stock
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Price
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Category
                </TableCell>
                <TableCell
                  isHeader
                  className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                >
                  Actions
                </TableCell>
              </TableRow>
            </TableHeader>

            <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
              {books.map((book) => (
                <BookRow
                  key={book.id}
                  book={book}
                  onEdit={() => handleOpenEditModal(book)}
                  onDelete={() => openModal(book.id, 'delete')}
                  languageName={languages[book.languageId]?.name || '�'}
                  categoryName={categories[book.categoryId]?.name || '�'}
                />
              ))}
            </TableBody>
          </Table>
          <div className="flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3 sm:px-6 dark:bg-white/[0.03]">
            <div>
              <p className="text-sm text-gray-700 dark:text-gray-400">
                Showing{' '}
                <span className="font-medium">{indexOfFirstBook + 1}</span> to{' '}
                <span className="font-medium">
                  {Math.min(indexOfLastBook, books.length)}
                </span>{' '}
                of <span className="font-medium">{books.length}</span> results
              </p>
            </div>

            <div>
              <nav
                className="isolate inline-flex -space-x-px rounded-md shadow-sm"
                aria-label="Pagination"
              >
                <button
                  onClick={() => goToPage(currentPage - 1)}
                  disabled={currentPage === 1}
                  className="relative inline-flex items-center rounded-l-md px-2 py-2 text-sm text-gray-500 ring-1 ring-gray-300 hover:bg-gray-50 disabled:opacity-50"
                >
                  <ChevronLeftIcon className="size-5" />
                </button>

                {[...Array(totalPages)].map((_, i) => {
                  const page = i + 1;
                  return (
                    <button
                      key={page}
                      onClick={() => goToPage(page)}
                      className={`relative inline-flex items-center px-4 py-2 text-sm font-medium ring-1 ring-gray-300 ${
                        page === currentPage
                          ? 'bg-brand-500 text-white'
                          : 'text-gray-900 hover:bg-gray-50'
                      }`}
                    >
                      {page}
                    </button>
                  );
                })}

                <button
                  onClick={() => goToPage(currentPage + 1)}
                  disabled={currentPage === totalPages}
                  className="relative inline-flex items-center rounded-r-md px-2 py-2 text-sm text-gray-500 ring-1 ring-gray-300 hover:bg-gray-50 disabled:opacity-50"
                >
                  <ChevronRightIcon className="size-5" />
                </button>
              </nav>
            </div>
          </div>
        </div>
      </div>
      {modalType === 'edit' && modalId && (
        <EditBookModal
          isOpen={isOpen}
          onClose={closeModal}
          editedBook={editedBook}
          setEditedBook={setEditedBook}
          onSubmit={(e) => {
            e.preventDefault();
            editBook(modalId, editedBook);
          }}
        />
      )}
      {modalType === 'delete' && modalId && (
        <DeleteBookModal
          isOpen={isOpen}
          onClose={closeModal}
          onConfirm={() => deleteBook(modalId)}
        />
      )}
    </div>
  );
}
