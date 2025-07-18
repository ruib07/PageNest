import type { IOrderRowProps } from '../../../types/order';
import { orderStatusLabels } from '../../../utils/dictionary';
import { TableCell, TableRow } from '../../ui/Table';

export default function OrderRow({ order }: IOrderRowProps) {
  return (
    <TableRow key={order.id}>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {order.userId}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
        {orderStatusLabels[order.status]}
      </TableCell>
      <TableCell className="px-4 py-3 text-gray-500 line-clamp-2 text-start text-theme-sm dark:text-gray-400">
        {order.total}
      </TableCell>
    </TableRow>
  );
}
