import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { EyeCloseIcon, EyeIcon } from '../../icons';
import { SignUp } from '../../services/authentications.service';
import type { ICreateUser } from '../../types/authentication';
import { showErrorToast } from '../../utils/toast-helper';
import ComponentCard from '../common/ComponentCard';
import Form from '../form/Form';
import Label from '../form/Label';
import Input from '../form/input/InputField';

export default function SignUpForm() {
  const [name, setName] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [showPassword, setShowPassword] = useState(false);
  const [errors, setErrors] = useState<{
    name?: string;
    email?: string;
    password?: string;
  }>({});
  const navigate = useNavigate();

  const validateFields = () => {
    const newErrors: { name?: string; email?: string; password?: string } =
      {};

    if (!name.trim()) newErrors.name = 'Name is required.';
    if (!email.trim()) newErrors.email = 'Valid email is required.';
    if (!password.trim()) newErrors.password = 'Password is required.';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});

    if (!validateFields()) return;

    const newUser: ICreateUser = {
      name,
      email,
      password,
      role: 1,
    };

    try {
      await SignUp(newUser);
      navigate('/signin');
    } catch (error: any) {
      const message = error.response?.data;

      if (!message || typeof message !== 'string') {
        showErrorToast();
        return;
      }

      const lowerMsg = message.toLowerCase();

      if (lowerMsg.includes('name')) {
        setErrors({ name: message });
      } else if (lowerMsg.includes('email')) {
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
          <ComponentCard title="Create Account">
            <div>
              <Form onSubmit={handleSubmit}>
                <div className="space-y-5">
                  <div>
                    <Label>
                      Name<span className="text-error-500">*</span>
                    </Label>
                    <Input
                      type="text"
                      placeholder="Type your name"
                      value={name}
                      onChange={(e) => setName(e.target.value)}
                      error={!!errors.name}
                      errorMessage={errors.name}
                    />
                  </div>
                  <div>
                    <Label>
                      Email<span className="text-error-500">*</span>
                    </Label>
                    <Input
                      type="email"
                      placeholder="Type your email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      error={!!errors.email}
                      errorMessage={errors.email}
                    />
                  </div>
                  <div>
                    <Label>
                      Password<span className="text-error-500">*</span>
                    </Label>
                    <div className="relative">
                      <Input
                        placeholder="Type a password"
                        type={showPassword ? 'text' : 'password'}
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
                          <EyeIcon className="fill-gray-500 dark:fill-gray-400 size-5" />
                        ) : (
                          <EyeCloseIcon className="fill-gray-500 dark:fill-gray-400 size-5" />
                        )}
                      </span>
                    </div>
                  </div>
                  <div>
                    <button className="flex items-center justify-center w-full px-4 py-3 text-sm font-medium text-white transition rounded-lg bg-brand-500 shadow-theme-xs hover:bg-brand-600">
                      Create Account
                    </button>
                  </div>
                </div>
              </Form>

              <div className="mt-5">
                <p className="text-sm font-normal text-center text-gray-700 dark:text-gray-400 sm:text-start">
                  Already have an account? {''}
                  <Link
                    to="/signin"
                    className="text-brand-500 hover:text-brand-600 dark:text-brand-400"
                  >
                    Authenticate
                  </Link>
                </p>
              </div>
            </div>
          </ComponentCard>
        </div>
      </div>
    </div>
  );
}
