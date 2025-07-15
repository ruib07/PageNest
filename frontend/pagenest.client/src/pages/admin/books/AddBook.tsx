import AddBookForm from '../../../components/admin/books/AddBookForm';
import PageMeta from '../../../components/common/PageMeta';

export default function AddBook() {
    return (
        <>
            <PageMeta
                title="Add Book Form"
                description="This is the book creation page"
            />
            <AddBookForm />
        </>
    );
}