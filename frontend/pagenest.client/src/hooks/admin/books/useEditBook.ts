import { useState } from 'react';
import { UpdateBook } from '../../../services/books.service';
import type { IBook, IUseEditBookProps } from '../../../types/book';

export function useEditBook({
    books,
    setBooks,
    closeModal,
    setError,
}: IUseEditBookProps) {
    const [isSubmitting, setIsSubmitting] = useState(false);

    const editBook = async (
        modalId: string,
        editedBook: Partial<IBook>
    ) => {
        if (!modalId) return;

        const updatedBook: IBook = { ...editedBook } as IBook;

        try {
            setIsSubmitting(true);

            books.find((book) => book.id === modalId);

            await UpdateBook(modalId, updatedBook);

            setBooks((prev) =>
                prev.map((book) =>
                    book.id === modalId ? { ...book, ...updatedBook } : book
                )
            );

            closeModal();
        } catch {
            setError('Error updating book.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return { editBook, isSubmitting };
}