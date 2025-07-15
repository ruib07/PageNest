import { useEffect, useState } from "react";
import { GetLanguages } from "../../../services/languages.service";
import type { ILanguage } from "../../../types/language";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "../../ui/Table";
import LanguageRow from "./LanguageRow";

export default function LanguagesTable() {
    const [languages, setLanguages] = useState<ILanguage[]>([]);
    const [, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchLanguages = async () => {
            try {
                const response = await GetLanguages();
                setLanguages(response.data);
            } catch {
                setError('Failed to fetch languages.');
            }
        };

        fetchLanguages();
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
                                    Name
                                </TableCell>
                                <TableCell
                                    isHeader
                                    className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    Code
                                </TableCell>
                                <TableCell
                                    isHeader
                                    className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400"
                                >
                                    Culture Code
                                </TableCell>
                            </TableRow>
                        </TableHeader>

                        <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                            {languages.map((language) => (
                                <LanguageRow
                                    key={language.id}
                                    language={language}
                                />
                            ))}
                        </TableBody>
                    </Table>
                </div>
            </div>
        </div>
    );
}