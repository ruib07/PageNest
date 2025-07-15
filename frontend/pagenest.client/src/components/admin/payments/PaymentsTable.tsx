import { useEffect, useState } from "react";
import { GetPayments } from "../../../services/payments.service";
import type { IPayment } from "../../../types/payment";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "../../ui/Table";
import PaymentRow from "./PaymentRow";

export default function PaymentsTable() {
    const [payments, setPayments] = useState<IPayment[]>([]);
    const [, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchPayments = async () => {
            try {
                const response = await GetPayments();
                setPayments(response.data);
            } catch {
                setError('Failed to fetch payments.');
            }
        };

        fetchPayments();
    }, []);

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
                                    Order ID
                                </TableCell>
                                <TableCell
                                    isHeader
                                    className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    Amount
                                </TableCell>
                                <TableCell
                                    isHeader
                                    className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    Stripe Payment Intent ID
                                </TableCell>
                                <TableCell
                                    isHeader
                                    className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    Status
                                </TableCell>
                                <TableCell
                                    isHeader
                                    className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    Created At
                                </TableCell>
                            </TableRow>
                        </TableHeader>

                        <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                            {payments.map((payment) => (
                                <PaymentRow
                                    key={payment.id}
                                    payment={payment}
                                />
                            ))}
                        </TableBody>
                    </Table>
                </div>
            </div>
        </div>
    );
}