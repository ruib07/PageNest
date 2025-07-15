import LanguagesTable from '../../components/admin/languages/LanguagesTable';
import ComponentCard from '../../components/common/ComponentCard';
import PageBreadcrumb from '../../components/common/PageBreadCrumb';
import PageMeta from '../../components/common/PageMeta';

export default function Languages() {
    return (
        <>
            <PageMeta
                title="Languages Table"
                description="This is the languages table to see all the available languages for the authenticated admin"
            />
            <PageBreadcrumb pageTitle="Languages" />
            <div className="space-y-6">
                <ComponentCard title="">
                    <div className="flex items-center justify-between">
                        <h2 className="text-lg font-semibold text-gray-700 dark:text-gray-400">
                            All Languages
                        </h2>
                    </div>
                    <LanguagesTable />
                </ComponentCard>
            </div>
        </>
    );
}