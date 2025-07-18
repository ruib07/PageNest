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

export interface IBookRowProps {
  book: IBook;
  onEdit: () => void;
  onDelete: () => void;
  languageName: string;
  categoryName: string;
}

export interface IEditBookModalProps {
  isOpen: boolean;
  onClose: () => void;
  editedBook: Partial<IBook>;
  setEditedBook: (book: Partial<IBook>) => void;
  onSubmit: (e: React.FormEvent) => void;
}

export interface IDeleteBookModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export interface IUseEditBookProps {
  books: IBook[];
  setBooks: React.Dispatch<React.SetStateAction<IBook[]>>;
  closeModal: () => void;
  setError: (error: string | null) => void;
}

export interface IUseDeleteBookProps {
  books: IBook[];
  setBooks: React.Dispatch<React.SetStateAction<IBook[]>>;
  closeModal: () => void;
  setError: (error: string | null) => void;
}
