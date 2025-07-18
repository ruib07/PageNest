import { PencilIcon, TrashBinIcon } from '../../../icons';
import type { IBookRowProps } from '../../../types/book';
import { TableCell, TableRow } from '../../ui/Table';

export default function BookRow({
  book,
  onEdit,
  onDelete,
  languageName,
  categoryName,
}: IBookRowProps) {
  const buttonClass =
    'flex w-full items-center justify-center gap-2 rounded-full bg-brand-500 px-4 py-3 text-sm font-medium text-white shadow-theme-xs hover:bg-brand-600 lg:inline-flex lg:w-auto';

  return (
    <TableRow key={book.id}>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        <img
          src={book.coverImageUrl}
          alt={book.title}
          className="w-16 h-16 object-cover rounded-md border border-gray-300 dark:border-gray-700"
        />
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {book.title}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {book.author}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 line-clamp-2 text-start text-theme-sm dark:text-gray-400">
        {book.description}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {new Date(book.publishedDate).toLocaleString().slice(0, -3)}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {book.isbn}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {book.pageCount}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {languageName}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {book.stock}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {book.price}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {categoryName}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        <div className="flex gap-2">
          <button onClick={onEdit} className={buttonClass}>
            <PencilIcon className="fill-gray-500 dark:fill-gray-400 size-5" />
          </button>
          <button onClick={onDelete} className={buttonClass}>
            <TrashBinIcon className="fill-gray-500 dark:fill-gray-400 size-5" />
          </button>
        </div>
      </TableCell>
    </TableRow>
  );
}
