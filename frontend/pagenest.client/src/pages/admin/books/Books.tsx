import { useNavigate } from 'react-router-dom';
import BooksTable from '../../../components/admin/books/BooksTable';
import ComponentCard from '../../../components/common/ComponentCard';
import PageBreadcrumb from '../../../components/common/PageBreadCrumb';
import PageMeta from '../../../components/common/PageMeta';

export default function Books() {
    const navigate = useNavigate();

    return (
        <>
            <PageMeta
                title="Books Table"
                description="This is the books table to see all the available books for the authenticated admin"
            />
            <PageBreadcrumb pageTitle="Books" />
            <div className="space-y-6">
                <ComponentCard title="">
                    <div className="flex items-center justify-between">
                        <h2 className="text-lg font-semibold text-gray-700 dark:text-gray-400">
                            All Books
                        </h2>
                        <button
                            onClick={() => navigate('/admin/add-book')}
                            className="inline-flex items-center gap-2 rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-theme-sm font-medium text-gray-700 shadow-theme-xs hover:bg-brand-400 hover:text-white dark:border-gray-700 dark:bg-brand-500 dark:text-gray-200 dark:hover:bg-brand-600 dark:hover:text-gray-200"
                        >
                            Add Book
                        </button>
                    </div>
                    <BooksTable />
                </ComponentCard>
            </div>
        </>
    );
}