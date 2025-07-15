import type { ILanguageRowProps } from '../../../types/language';
import { TableCell, TableRow } from '../../ui/Table';

export default function LanguageRow({
    language,
}: ILanguageRowProps) {
    return (
        <TableRow key={language.id}>
            <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
                {language.name}
            </TableCell>
            <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
                {language.code}
            </TableCell>
            <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
                {language.cultureCode}
            </TableCell>
        </TableRow>
    );
}