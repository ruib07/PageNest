import type { IPaymentRowProps } from '../../../types/payment';
import { TableCell, TableRow } from '../../ui/Table';

export default function PaymentRow({
    payment,
}: IPaymentRowProps) {
    return (
        <TableRow key={payment.id}>
            <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
                {payment.orderId}
            </TableCell>
            <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
                {payment.amount}
            </TableCell>
            <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
                {payment.stripePaymentIntentId}
            </TableCell>
            <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
                {payment.status}
            </TableCell>
            <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
                {new Date(payment.createdAt!).toLocaleString().slice(0, -3)}
            </TableCell>
        </TableRow>
    );
}