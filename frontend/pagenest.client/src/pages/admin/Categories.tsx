import CategoriesTable from '../../components/admin/categories/CategoriesTable';
import ComponentCard from '../../components/common/ComponentCard';
import PageBreadcrumb from '../../components/common/PageBreadCrumb';
import PageMeta from '../../components/common/PageMeta';

export default function Categories() {
    return (
        <>
            <PageMeta
                title="Categories Table"
                description="This is the categories table to see all the available categories for the authenticated admin"
            />
            <PageBreadcrumb pageTitle="Categories" />
            <div className="space-y-6">
                <ComponentCard title="">
                    <div className="flex items-center justify-between">
                        <h2 className="text-lg font-semibold text-gray-700 dark:text-gray-400">
                            All Categories
                        </h2>
                    </div>
                    <CategoriesTable />
                </ComponentCard>
            </div>
        </>
    );
}