import GenresTable from '../../components/admin/genres/GenresTable';
import ComponentCard from '../../components/common/ComponentCard';
import PageBreadcrumb from '../../components/common/PageBreadCrumb';
import PageMeta from '../../components/common/PageMeta';

export default function Genres() {
    return (
        <>
            <PageMeta
                title="Genres Table"
                description="This is the genres table to see all the available genres for the authenticated admin"
            />
            <PageBreadcrumb pageTitle="Genres" />
            <div className="space-y-6">
                <ComponentCard title="">
                    <div className="flex items-center justify-between">
                        <h2 className="text-lg font-semibold text-gray-700 dark:text-gray-400">
                            All Genres
                        </h2>
                    </div>
                    <GenresTable />
                </ComponentCard>
            </div>
        </>
    );
}