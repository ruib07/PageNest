import type { IGenreRowProps } from '../../../types/genre';
import { TableCell, TableRow } from '../../ui/Table';

export default function GenreRow({
    genre,
}: IGenreRowProps) {
    return (
        <TableRow key={genre.id}>
            <TableCell className="px-4 py-3 text-gray-500 text-start text-theme-sm dark:text-gray-400">
                {genre.name}
            </TableCell>
        </TableRow>
    );
}