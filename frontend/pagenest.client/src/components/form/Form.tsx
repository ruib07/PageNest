import type { ReactNode, FormEvent } from 'react';

interface FormProps {
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  children: ReactNode;
  className?: string;
}

export default function Form({ onSubmit, children, className }: FormProps) {
  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit(event);
      }}
      className={` ${className}`}
    >
      {children}
    </form>
  );
}
