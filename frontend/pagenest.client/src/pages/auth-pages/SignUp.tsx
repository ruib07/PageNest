import SignUpForm from '../../components/authentication/SignUpForm';
import PageMeta from '../../components/common/PageMeta';

export default function SignUp() {
  return (
    <>
      <PageMeta
        title="SignUp Form"
        description="This is the page to create a employee"
      />
      <SignUpForm />
    </>
  );
}
