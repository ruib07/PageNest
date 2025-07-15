import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { CreateBook } from '../../../services/books.service';
import { GetCategories } from '../../../services/categories.service';
import { GetLanguages } from '../../../services/languages.service';
import type { IBook } from '../../../types/book';
import { showErrorToast, showSuccessToast } from '../../../utils/toast-helper';
import ComponentCard from '../../common/ComponentCard';
import Form from '../../form/Form';
import Label from '../../form/Label';
import Select from '../../form/Select';
import Input from '../../form/input/InputField';
import TextArea from '../../form/input/TextArea';
import Button from '../../ui/Button';

export default function AddBookForm() {
    const [title, setTitle] = useState<string>('');
    const [author, setAuthor] = useState<string>('');
    const [description, setDescription] = useState<string>('');
    const [publishedDate, setPublishedDate] = useState<string>('');
    const [isbn, setISBN] = useState<string>('');
    const [pageCount, setPageCount] = useState<string>('');
    const [languageId, setLanguageId] = useState<string | number>('');
    const [coverImageUrl, setCoverImageURL] = useState<string>('');
    const [stock, setStock] = useState<string>('');
    const [price, setPrice] = useState<string>('');
    const [categoryId, setCategoryId] = useState<string | number>('');
    const [languages, setLanguages] = useState<{ value: string; label: string }[]>([]);
    const [categories, setCategories] = useState<{ value: string; label: string }[]>([]);
    const [, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchData = async () => {
            try {
                const languagesResponse = await GetLanguages();
                setLanguages(languagesResponse.data.map((s: any) => ({ value: s.id, label: s.name })));

                const categoriesResponse = await GetCategories();
                setCategories(categoriesResponse.data.map((c: any) => ({ value: c.id, label: c.name })));
            } catch {
                setError("Failed to load data.");
            }
        };

        fetchData();
    });

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const newBook: IBook = {
            title,
            author,
            description,
            publishedDate: new Date(publishedDate),
            isbn,
            pageCount: parseInt(pageCount, 10),
            languageId: languageId.toString(),
            coverImageUrl,
            stock: parseInt(stock, 10),
            price: parseFloat(price),
            categoryId: categoryId.toString(),
        };

        try {
            await CreateBook(newBook);
            showSuccessToast();
            navigate('/admin/books');
        } catch {
            showErrorToast();
        }
    };

    return (
        <ComponentCard title="Book Creation">
            <Form onSubmit={handleSubmit}>
                <div className="space-y-6">
                    <div>
                        <Label htmlFor="input">Title</Label>
                        <Input
                            type="text"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">Author</Label>
                        <Input
                            type="text"
                            value={author}
                            onChange={(e) => setAuthor(e.target.value)}
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">Description</Label>
                        <TextArea
                            value={description}
                            onChange={setDescription}
                            rows={4}
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">Published Date</Label>
                        <Input
                            type="datetime-local"
                            value={publishedDate}
                            onChange={(e) => setPublishedDate(e.target.value)}
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">ISBN</Label>
                        <Input
                            type="text"
                            value={isbn}
                            onChange={(e) => setISBN(e.target.value)}
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">Page Count</Label>
                        <Input
                            type="number"
                            value={pageCount}
                            onChange={(e) => setPageCount(e.target.value)}
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">Language</Label>
                        <Select
                            options={languages}
                            placeholder="Select a language"
                            onChange={setLanguageId}
                            className="dark:bg-dark-900"
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">Cover Image URL</Label>
                        <Input
                            type="text"
                            value={coverImageUrl}
                            onChange={(e) => setCoverImageURL(e.target.value)}
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">Stock</Label>
                        <Input
                            type="number"
                            value={stock}
                            onChange={(e) => setStock(e.target.value)}
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">Price</Label>
                        <Input
                            type="number"
                            value={price}
                            onChange={(e) => setPrice(e.target.value)}
                        />
                    </div>

                    <div>
                        <Label htmlFor="input">Category</Label>
                        <Select
                            options={categories}
                            placeholder="Select a category"
                            onChange={setCategoryId}
                            className="dark:bg-dark-900"
                        />
                    </div>
                    <div className="text-center">
                        <Button className="w-sm" size="sm" type="submit">
                            Create Book
                        </Button>
                    </div>
                </div>
            </Form>
        </ComponentCard>
    );
}