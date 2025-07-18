import { toast } from 'react-toastify';

export const showToast = (message: string, type: 'success' | 'error') => {
  toast[type](message, {
    position: 'bottom-right',
    autoClose: 5000,
    closeOnClick: true,
    draggable: true,
  });
};

export const showSuccessToast = (message = 'Success') => {
  showToast(message, 'success');
};

export const showErrorToast = (message = 'Something went wrong') => {
  showToast(message, 'error');
};
