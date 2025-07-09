import { HeadProvider, Title, Meta } from 'react-head';

const PageMeta = ({
  title,
  description,
}: {
  title: string;
  description: string;
}) => (
  <>
    <Title>{title}</Title>
    <Meta name="description" content={description} />
  </>
);

export const AppWrapper = ({ children }: { children: React.ReactNode }) => (
  <HeadProvider>{children}</HeadProvider>
);

export default PageMeta;
