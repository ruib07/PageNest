import type { IDeleteBookModalProps } from '../../../types/book';
import Form from '../../form/Form';
import Button from '../../ui/Button';
import Modal from '../../ui/Modal';

export default function DeleteBookModal({
    isOpen,
    onClose,
    onConfirm,
}: IDeleteBookModalProps) {
    return (
        <Modal isOpen={isOpen} onClose={onClose} className="max-w-[700px] m-4">
            <div className="no-scrollbar relative w-full max-w-[700px] overflow-y-auto rounded-3xl bg-white p-4 dark:bg-gray-900 lg:p-11">
                <div className="px-2 pr-14 text-center">
                    <h4 className="mb-2 text-2xl font-semibold text-gray-800 dark:text-white/90">
                        Delete Book
                    </h4>
                    <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
                        Are you sure that you want to delete <strong>this book</strong>?
                    </p>
                </div>
                <Form className="flex flex-col" onSubmit={onConfirm}>
                    <div className="flex items-center gap-3 px-2 lg:justify-center">
                        <Button size="sm" type="submit">
                            Yes, delete
                        </Button>
                        <Button size="sm" variant="outline" onClick={onClose}>
                            No
                        </Button>
                    </div>
                </Form>
            </div>
        </Modal>
    );
}