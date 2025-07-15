import { useEffect, useState } from "react";
import { GetGenres } from "../../../services/genres.service";
import type { IGenre } from "../../../types/genre";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "../../ui/Table";
import GenreRow from "./GenreRow";

export default function GenresTable() {
    const [genres, setGenres] = useState<IGenre[]>([]);
    const [, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchGenres = async () => {
            try {
                const response = await GetGenres();
                setGenres(response.data);
            } catch {
                setError('Failed to fetch genres.');
            }
        };

        fetchGenres();
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
                            </TableRow>
                        </TableHeader>

                        <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                            {genres.map((genre) => (
                                <GenreRow
                                    key={genre.id}
                                    genre={genre}
                                />
                            ))}
                        </TableBody>
                    </Table>
                </div>
            </div>
        </div>
    );
}