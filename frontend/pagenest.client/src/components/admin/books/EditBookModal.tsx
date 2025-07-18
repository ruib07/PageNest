import { useEffect, useState } from 'react';
import { GetCategories } from '../../../services/categories.service';
import { GetLanguages } from '../../../services/languages.service';
import type { IEditBookModalProps } from '../../../types/book';
import Form from '../../form/Form';
import Input from '../../form/input/InputField';
import TextArea from '../../form/input/TextArea';
import Label from '../../form/Label';
import Select from '../../form/Select';
import Button from '../../ui/Button';
import Modal from '../../ui/Modal';

export default function EditBookModal({
  isOpen,
  onClose,
  editedBook,
  setEditedBook,
  onSubmit,
}: IEditBookModalProps) {
  const [errors, setErrors] = useState<{
    title?: string;
    author?: string;
    description?: string;
    publishedDate?: string;
    isbn?: string;
    pageCount?: string;
    languageId?: string;
    coverImageUrl?: string;
    stock?: string;
    price?: string;
    categoryId?: string;
  }>({});
  const [, setError] = useState<string | null>(null);
  const [updateLanguages, setUpdateLanguages] = useState<
    { value: string; label: string }[]
  >([]);
  const [updateCategories, setUpdateCategories] = useState<
    { value: string; label: string }[]
  >([]);

  const validateFields = () => {
    const newErrors: {
      title?: string;
      author?: string;
      description?: string;
      publishedDate?: string;
      isbn?: string;
      pageCount?: string;
      languageId?: string;
      coverImageUrl?: string;
      stock?: string;
      price?: string;
      categoryId?: string;
    } = {};

    if (!editedBook.title?.trim()) newErrors.title = 'Name is required.';
    if (!editedBook.author?.trim()) newErrors.author = 'Author is required.';
    if (!editedBook.description?.trim())
      newErrors.description = 'Description is required.';
    if (!editedBook.publishedDate)
      newErrors.publishedDate = 'Published date is required.';
    if (!editedBook.isbn?.trim()) newErrors.isbn = 'ISBN is required.';
    if (!editedBook.pageCount) newErrors.pageCount = 'Page Count is required.';
    if (!editedBook.languageId) newErrors.languageId = 'Language is required.';
    if (!editedBook.coverImageUrl?.trim())
      newErrors.coverImageUrl = 'Cover Image URL is required.';
    if (!editedBook.stock) newErrors.stock = 'Stock is required.';
    if (!editedBook.price) newErrors.price = 'Price is required.';
    if (!editedBook.categoryId) newErrors.categoryId = 'Category is required.';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  useEffect(() => {
    const fetchLanguagesAndCategories = async () => {
      try {
        const languagesResponse = await GetLanguages();
        setUpdateLanguages(
          languagesResponse.data.map((lang: any) => ({
            value: String(lang.id),
            label: lang.name,
          }))
        );

        const categoriesResponse = await GetCategories();
        setUpdateCategories(
          categoriesResponse.data.map((cat: any) => ({
            value: String(cat.id),
            label: cat.name,
          }))
        );
      } catch {
        setError('Failed to load languages and/or categories.');
      }
    };

    fetchLanguagesAndCategories();
  }, []);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (validateFields()) {
      onSubmit(e);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} className="max-w-[700px] m-4">
      <div className="no-scrollbar relative w-full max-w-[700px] overflow-y-auto rounded-3xl bg-white p-4 dark:bg-gray-900 lg:p-11">
        <div className="px-2 pr-14">
          <h4 className="mb-2 text-2xl font-semibold text-gray-800 dark:text-white/90">
            Edit Book
          </h4>
          <p className="mb-6 text-sm text-gray-500 dark:text-gray-400 lg:mb-7">
            You can edit the book details here.
          </p>
        </div>
        <Form className="flex flex-col" onSubmit={handleSubmit}>
          <div className="custom-scrollbar overflow-y-auto px-2 pb-3">
            <div className="grid grid-cols-1 gap-x-6 gap-y-5 lg:grid-cols-2">
              <div className="col-span-2 lg:col-span-2">
                <Label>Title</Label>
                <Input
                  type="text"
                  value={editedBook.title || ''}
                  onChange={(e: any) =>
                    setEditedBook({
                      ...editedBook,
                      title: e.target.value,
                    })
                  }
                  error={!!errors.title}
                  errorMessage={errors.title}
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>Author</Label>
                <Input
                  type="text"
                  value={editedBook.author || ''}
                  onChange={(e: any) =>
                    setEditedBook({
                      ...editedBook,
                      author: e.target.value,
                    })
                  }
                  error={!!errors.author}
                  errorMessage={errors.author}
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>Description</Label>
                <TextArea
                  value={editedBook.description || ''}
                  onChange={(e: any) =>
                    setEditedBook({
                      ...editedBook,
                      description: e.target.value,
                    })
                  }
                  rows={4}
                  error={!!errors.description}
                  errorMessage={errors.description}
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>Published Date</Label>
                <Input
                  type="datetime-local"
                  value={
                    editedBook.publishedDate
                      ? typeof editedBook.publishedDate === 'string'
                        ? editedBook.publishedDate
                        : new Date(editedBook.publishedDate)
                            .toISOString()
                            .slice(0, 16)
                      : ''
                  }
                  onChange={(e: any) =>
                    setEditedBook({
                      ...editedBook,
                      publishedDate: e.target.value,
                    })
                  }
                  error={!!errors.publishedDate}
                  errorMessage={errors.publishedDate}
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>ISBN</Label>
                <Input
                  type="text"
                  value={editedBook.isbn || ''}
                  onChange={(e: any) =>
                    setEditedBook({
                      ...editedBook,
                      isbn: e.target.value,
                    })
                  }
                  error={!!errors.isbn}
                  errorMessage={errors.isbn}
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>Page Count</Label>
                <Input
                  type="number"
                  value={editedBook.pageCount || ''}
                  onChange={(e: any) =>
                    setEditedBook({
                      ...editedBook,
                      pageCount: e.target.value,
                    })
                  }
                  error={!!errors.pageCount}
                  errorMessage={errors.pageCount}
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>Language</Label>
                <Select
                  options={updateLanguages}
                  placeholder="Select a language"
                  onChange={(e) =>
                    setEditedBook({ ...editedBook, languageId: String(e) })
                  }
                  className="dark:bg-dark-900"
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>Cover Image URL</Label>
                <Input
                  type="text"
                  value={editedBook.coverImageUrl || ''}
                  onChange={(e: any) =>
                    setEditedBook({
                      ...editedBook,
                      coverImageUrl: e.target.value,
                    })
                  }
                  error={!!errors.coverImageUrl}
                  errorMessage={errors.coverImageUrl}
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>Stocl</Label>
                <Input
                  type="number"
                  value={editedBook.stock || ''}
                  onChange={(e: any) =>
                    setEditedBook({
                      ...editedBook,
                      stock: e.target.value,
                    })
                  }
                  error={!!errors.stock}
                  errorMessage={errors.stock}
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>Price</Label>
                <Input
                  type="number"
                  value={editedBook.price || ''}
                  onChange={(e: any) =>
                    setEditedBook({
                      ...editedBook,
                      price: e.target.value,
                    })
                  }
                  error={!!errors.price}
                  errorMessage={errors.price}
                />
              </div>

              <div className="col-span-2 lg:col-span-2">
                <Label>Category</Label>
                <Select
                  options={updateCategories}
                  placeholder="Select a category"
                  onChange={(e) =>
                    setEditedBook({ ...editedBook, categoryId: String(e) })
                  }
                  className="dark:bg-dark-900"
                />
              </div>
            </div>
          </div>
          <div className="flex items-center gap-3 px-2 mt-6 lg:justify-end">
            <Button size="sm" variant="outline" onClick={onClose}>
              Close
            </Button>
            <Button size="sm" type="submit">
              Save Changes
            </Button>
          </div>
        </Form>
      </div>
    </Modal>
  );
}
