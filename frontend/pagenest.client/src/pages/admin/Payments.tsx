import PaymentsTable from '../../components/admin/payments/PaymentsTable';
import ComponentCard from '../../components/common/ComponentCard';
import PageBreadcrumb from '../../components/common/PageBreadCrumb';
import PageMeta from '../../components/common/PageMeta';

export default function Payments() {
    return (
        <>
            <PageMeta
                title="Payments Table"
                description="This is the payments table to see all the available payments for the authenticated admin"
            />
            <PageBreadcrumb pageTitle="Payments" />
            <div className="space-y-6">
                <ComponentCard title="">
                    <div className="flex items-center justify-between">
                        <h2 className="text-lg font-semibold text-gray-700 dark:text-gray-400">
                            All Payments
                        </h2>
                    </div>
                    <PaymentsTable />
                </ComponentCard>
            </div>
        </>
    );
}