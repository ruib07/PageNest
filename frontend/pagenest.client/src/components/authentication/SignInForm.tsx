import { jwtDecode } from 'jwt-decode';
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { EyeCloseIcon, EyeIcon } from '../../icons';
import { Authentication } from '../../services/authentications.service';
import { GetUserById } from '../../services/users.service';
import type {
  IDecodedToken,
  ILoginCredentials,
} from '../../types/authentication';
import { showErrorToast } from '../../utils/toast-helper';
import ComponentCard from '../common/ComponentCard';
import Form from '../form/Form';
import Input from '../form/input/InputField';
import Label from '../form/Label';
import Button from '../ui/Button';

export default function SignInForm() {
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [showPassword, setShowPassword] = useState(false);
  const [errors, setErrors] = useState<{ email?: string; password?: string }>(
    {}
  );
  const navigate = useNavigate();
  const { login } = useAuth();

  const validateFields = () => {
    const newErrors: { email?: string; password?: string } = {};

    if (!email.trim()) newErrors.email = 'Email is required.';
    if (!password.trim()) newErrors.password = 'Password is required.';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});

    if (!validateFields()) return;

    const user: ILoginCredentials = { email, password };

    try {
      const response = await Authentication(user);
      const token = response.data.accessToken;
      const decodedToken: IDecodedToken = jwtDecode(token);

      localStorage.setItem('refreshToken', response.data.refreshToken);

      const userResponse = await GetUserById(decodedToken.id);
      const userInfo = userResponse.data;

      login({
        id: userInfo.id,
        name: userInfo.name,
        email: userInfo.email,
        role: userInfo.role,
        token,
      });

      navigate('/');
    } catch (error: any) {
      const message = error.response?.data;

      if (!message || typeof message !== 'string') {
        showErrorToast();
        return;
      }

      const lowerMsg = message.toLowerCase();

      if (lowerMsg.includes('email')) {
        setErrors({ email: message });
      } else if (lowerMsg.includes('password')) {
        setErrors({ password: message });
      } else {
        showErrorToast(message);
      }
    }
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen">
      <div className="flex flex-col justify-center flex-1 w-full max-w-md mx-auto">
        <div>
          <ComponentCard title="Authentication">
            <p className="text-sm text-gray-500">
              Type your email and password to authenticate!
            </p>
            <Form onSubmit={handleSubmit}>
              <div className="space-y-6">
                <div>
                  <Label>
                    Email <span className="text-error-500">*</span>{' '}
                  </Label>
                  <Input
                    placeholder="Type your email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    error={!!errors.email}
                    errorMessage={errors.email}
                  />
                </div>
                <div>
                  <Label>
                    Password <span className="text-error-500">*</span>{' '}
                  </Label>
                  <div className="relative">
                    <Input
                      type={showPassword ? 'text' : 'password'}
                      placeholder="Type your password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      error={!!errors.password}
                      errorMessage={errors.password}
                    />
                    <span
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute z-30 -translate-y-1/2 cursor-pointer right-4 top-1/2"
                    >
                      {showPassword ? (
                        <EyeIcon className="fill-gray-500 size-5" />
                      ) : (
                        <EyeCloseIcon className="fill-gray-500 size-5" />
                      )}
                    </span>
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <Link
                    to="http://localhost:3001/send-email"
                    className="text-sm text-brand-500 hover:text-brand-600 dark:text-brand-400"
                  >
                    Forgot your password?
                  </Link>
                </div>
                <div>
                  <Button className="w-full" size="sm" type="submit">
                    Authenticate
                  </Button>
                </div>
              </div>
            </Form>

            <div className="mt-5">
              <p className="text-sm font-normal text-center text-gray-700 dark:text-gray-400 sm:text-start">
                Don´t have an account? {''}
                <Link
                  to="/signup"
                  className="text-brand-500 hover:text-brand-600 dark:text-brand-400"
                >
                  Create Account
                </Link>
              </p>
            </div>
          </ComponentCard>
        </div>
      </div>
    </div>
  );
}
