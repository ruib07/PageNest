import { useState } from 'react';
import { DeleteBook } from '../../../services/books.service';
import type { IUseDeleteBookProps } from '../../../types/book';

export function useDeleteBook({
    books,
    setBooks,
    closeModal,
    setError,
}: IUseDeleteBookProps) {
    const [isDeleting, setIsDeleting] = useState(false);

    const deleteBook = async (modalId: string) => {
        if (!modalId) return;

        try {
            setIsDeleting(true);

            await DeleteBook(modalId);

            setBooks((prev) => prev.filter((book) => book.id !== modalId));
            closeModal();
        } catch {
            setError('Error deleting book.');
        } finally {
            setIsDeleting(false);
        }
    };

    return { deleteBook, isDeleting };
}