import { useState, useCallback } from 'react';

type ModalType = 'edit' | 'delete' | null;

export const useModal = (initialState: boolean = false) => {
  const [isOpen, setIsOpen] = useState(initialState);
  const [modalId, setModalId] = useState<string | null>(null);
  const [modalType, setModalType] = useState<ModalType>(null);

  const openModal = useCallback(
    (id: string | null = null, type: ModalType = null) => {
      setModalId(id);
      setModalType(type);
      setIsOpen(true);
    },
    []
  );

  const closeModal = useCallback(() => {
    setIsOpen(false);
    setModalId(null);
    setModalType(null);
  }, []);

  const toggleModal = useCallback(() => {
    setIsOpen((prev) => !prev);
  }, []);

  return { isOpen, modalId, modalType, openModal, closeModal, toggleModal };
};
