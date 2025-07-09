import PageMeta from '../components/common/PageMeta';
import { useAuth } from '../context/AuthContext';

export default function Dashboard() {
  const { user } = useAuth();

  if (!user) return;

  return (
    <>
      <PageMeta
        title="Dashboard"
        description="A dashboard page to show the ticket statistics"
      />
      <div className="justify-center p-4 sm:p-6">
        {user.role === 0 && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <p>Hello admin</p>
          </div>
        )}

        {user.role === 1 && (
          <>
            <p>Hello user</p>
          </>
        )}
      </div>
    </>
  );
}
