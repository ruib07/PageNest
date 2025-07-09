import SignInForm from '../../components/authentication/SignInForm';
import PageMeta from '../../components/common/PageMeta';

export default function SignIn() {
  return (
    <>
      <PageMeta
        title="SignIn Form"
        description="This is the page to authenticate"
      />
      <SignInForm />
    </>
  );
}
