import {
  Navigate,
  Outlet,
  Route,
  BrowserRouter as Router,
  Routes,
} from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

import ThemeToggler from './components/common/ThemeToggler';
import { useAuth } from './context/AuthContext';

import AppLayout from './layouts/AppLayout';
import Dashboard from './pages/Dashboard';
import NotFound from './pages/NotFound';

import ProtectedRoute from './routes/ProtectedRoute';

import SignIn from './pages/auth-pages/SignIn';
import SignUp from './pages/auth-pages/SignUp';

export default function App() {
  const { user } = useAuth();

  return (
    <Router>
      <ToastContainer />

      <div className="fixed z-50 bottom-6 right-6 hidden sm:block">
        <ThemeToggler />
      </div>

      <Routes>
        <Route path="/signup" element={<SignUp />} />
        <Route
          path="/signin"
          element={user ? <Navigate to="/" replace /> : <SignIn />}
        />
        <Route element={<AppLayout />}>
          <Route
            index
            path="/"
            element={user ? <Dashboard /> : <Navigate to="/signin" replace />}
          />

          <Route
            path="admin"
            element={
              <ProtectedRoute requiredRole={0}>
                <Outlet />
              </ProtectedRoute>
            }
          ></Route>

          <Route
            path="store"
            element={
              <ProtectedRoute requiredRole={1}>
                <Outlet />
              </ProtectedRoute>
            }
          ></Route>

          <Route path="/" element={<Dashboard />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </Router>
  );
}
