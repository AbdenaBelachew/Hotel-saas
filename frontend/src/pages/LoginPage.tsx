import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useMutation } from '@tanstack/react-query';
import { Navigate, useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { login } from '../api/auth';
import { getErrorMessage } from '../api/client';
import { useAuthStore } from '../stores/authStore';

const schema = z.object({
  email: z.string().email('Enter a valid email address.'),
  password: z.string().min(1, 'Password is required.')
});

type FormValues = z.infer<typeof schema>;

export function LoginPage() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const setAuth = useAuthStore((s) => s.setAuth);

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (result) => {
      setAuth(result);
      toast.success(`Welcome back, ${result.user.firstName}!`);
      navigate('/dashboard', { replace: true });
    },
    onError: (error) => toast.error(getErrorMessage(error))
  });

  if (user) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="d-flex align-items-center justify-content-center vh-100 bg-light">
      <div className="card shadow-sm" style={{ width: 380 }}>
        <div className="card-body p-4">
          <h1 className="h4 mb-1">Hotel PMS</h1>
          <p className="text-muted small mb-4">Sign in to your property management workspace.</p>

          <form onSubmit={handleSubmit((values) => mutation.mutate(values))} noValidate>
            <div className="mb-3">
              <label className="form-label" htmlFor="email">
                Email
              </label>
              <input
                id="email"
                type="email"
                className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                autoComplete="username"
                {...register('email')}
              />
              {errors.email && <div className="invalid-feedback">{errors.email.message}</div>}
            </div>

            <div className="mb-3">
              <label className="form-label" htmlFor="password">
                Password
              </label>
              <input
                id="password"
                type="password"
                className={`form-control ${errors.password ? 'is-invalid' : ''}`}
                autoComplete="current-password"
                {...register('password')}
              />
              {errors.password && <div className="invalid-feedback">{errors.password.message}</div>}
            </div>

            <button type="submit" className="btn btn-primary w-100" disabled={mutation.isPending}>
              {mutation.isPending ? 'Signing in…' : 'Sign in'}
            </button>
          </form>

          <div className="text-muted small mt-4 border-top pt-3">
            Demo logins (password <code>ChangeMe123!</code>): admin@demo.com, manager@demo.com, frontdesk@demo.com
          </div>
        </div>
      </div>
    </div>
  );
}
