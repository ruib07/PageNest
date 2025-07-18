import type { ICategoryRowProps } from '../../../types/category';
import { TableCell, TableRow } from '../../ui/Table';

export default function CategoryRow({ category }: ICategoryRowProps) {
  return (
    <TableRow key={category.id}>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {category.name}
      </TableCell>
    </TableRow>
  );
}
