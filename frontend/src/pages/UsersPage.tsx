import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { toast } from 'react-toastify';
import { createUser, getUsers, type CreateUserRequest } from '../api/users';
import { getRoles } from '../api/roles';
import { getProperties } from '../api/properties';
import { getErrorMessage } from '../api/client';
import { Modal } from '../components/Modal';
import { useAuthStore } from '../stores/authStore';

const schema = z.object({
  username: z.string().min(1, 'Username is required.'),
  email: z.string().email('Enter a valid email.'),
  password: z.string().min(8, 'At least 8 characters.'),
  firstName: z.string().min(1, 'First name is required.'),
  lastName: z.string().optional(),
  roleName: z.string().min(1, 'Select a role.'),
  propertyIds: z.array(z.string()).optional()
});

type FormValues = z.infer<typeof schema>;

export function UsersPage() {
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);
  const queryClient = useQueryClient();
  const isPlatformAdmin = useAuthStore((s) => s.isPlatformAdmin());

  const { data, isLoading } = useQuery({
    queryKey: ['users', page],
    queryFn: () => getUsers({ page, pageSize: 10 })
  });

  const rolesQuery = useQuery({ queryKey: ['roles'], queryFn: getRoles });
  const propertiesQuery = useQuery({
    queryKey: ['properties', 'all-for-assignment'],
    queryFn: () => getProperties({ page: 1, pageSize: 100 })
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const createMutation = useMutation({
    mutationFn: (values: FormValues) => createUser(values as CreateUserRequest),
    onSuccess: () => {
      toast.success('User created.');
      setShowCreate(false);
      reset();
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (error) => toast.error(getErrorMessage(error))
  });

  const assignableRoles = (rolesQuery.data ?? []).filter((r) => isPlatformAdmin || r.name !== 'PlatformAdmin');

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Users</h1>
        <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
          New User
        </button>
      </div>

      <div className="card">
        <div className="table-responsive">
          <table className="table table-hover mb-0 align-middle">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Role</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {isLoading && (
                <tr>
                  <td colSpan={4} className="text-center text-muted py-4">
                    Loading…
                  </td>
                </tr>
              )}
              {!isLoading && data?.items.length === 0 && (
                <tr>
                  <td colSpan={4} className="text-center text-muted py-4">
                    No users yet.
                  </td>
                </tr>
              )}
              {data?.items.map((u) => (
                <tr key={u.id}>
                  <td>
                    {u.firstName} {u.lastName}
                  </td>
                  <td>{u.email}</td>
                  <td>{u.roles.join(', ')}</td>
                  <td>
                    <span className={`badge ${u.isActive ? 'text-bg-success' : 'text-bg-secondary'}`}>
                      {u.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {data && data.totalPages > 1 && (
        <div className="d-flex justify-content-end gap-2 mt-3">
          <button className="btn btn-outline-secondary btn-sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
            Previous
          </button>
          <span className="align-self-center small text-muted">
            Page {data.page} of {data.totalPages}
          </span>
          <button
            className="btn btn-outline-secondary btn-sm"
            disabled={page >= data.totalPages}
            onClick={() => setPage((p) => p + 1)}
          >
            Next
          </button>
        </div>
      )}

      {showCreate && (
        <Modal title="New User" onClose={() => setShowCreate(false)}>
          <form onSubmit={handleSubmit((values) => createMutation.mutate(values))} noValidate>
            <div className="row">
              <div className="col mb-3">
                <label className="form-label">First name</label>
                <input className={`form-control ${errors.firstName ? 'is-invalid' : ''}`} {...register('firstName')} />
                {errors.firstName && <div className="invalid-feedback">{errors.firstName.message}</div>}
              </div>
              <div className="col mb-3">
                <label className="form-label">Last name</label>
                <input className="form-control" {...register('lastName')} />
              </div>
            </div>
            <div className="mb-3">
              <label className="form-label">Username</label>
              <input className={`form-control ${errors.username ? 'is-invalid' : ''}`} {...register('username')} />
              {errors.username && <div className="invalid-feedback">{errors.username.message}</div>}
            </div>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input className={`form-control ${errors.email ? 'is-invalid' : ''}`} {...register('email')} />
              {errors.email && <div className="invalid-feedback">{errors.email.message}</div>}
            </div>
            <div className="mb-3">
              <label className="form-label">Password</label>
              <input type="password" className={`form-control ${errors.password ? 'is-invalid' : ''}`} {...register('password')} />
              {errors.password && <div className="invalid-feedback">{errors.password.message}</div>}
            </div>
            <div className="mb-3">
              <label className="form-label">Role</label>
              <select className={`form-select ${errors.roleName ? 'is-invalid' : ''}`} {...register('roleName')} defaultValue="">
                <option value="" disabled>
                  Select a role…
                </option>
                {assignableRoles.map((r) => (
                  <option key={r.id} value={r.name}>
                    {r.name}
                  </option>
                ))}
              </select>
              {errors.roleName && <div className="invalid-feedback">{errors.roleName.message}</div>}
            </div>
            <div className="mb-3">
              <label className="form-label">Assigned properties</label>
              <select multiple className="form-select" size={4} {...register('propertyIds')}>
                {propertiesQuery.data?.items.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="d-flex justify-content-end gap-2">
              <button type="button" className="btn btn-outline-secondary" onClick={() => setShowCreate(false)}>
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={createMutation.isPending}>
                {createMutation.isPending ? 'Creating…' : 'Create'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}
