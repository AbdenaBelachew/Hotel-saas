import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { toast } from 'react-toastify';
import { createOrganization, getOrganizations, type CreateOrganizationRequest } from '../api/organizations';
import { getErrorMessage } from '../api/client';
import { Modal } from '../components/Modal';

const schema = z.object({
  name: z.string().min(1, 'Name is required.'),
  code: z
    .string()
    .min(1, 'Code is required.')
    .regex(/^[A-Za-z0-9-]+$/, 'Letters, digits and hyphens only.'),
  email: z.string().email('Enter a valid email.').optional().or(z.literal('')),
  currency: z.string().min(3, 'Use a 3-letter currency code.').max(3),
  timezone: z.string().min(1, 'Timezone is required.')
});

type FormValues = z.infer<typeof schema>;

export function OrganizationsPage() {
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['organizations', page],
    queryFn: () => getOrganizations({ page, pageSize: 10 })
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { currency: 'ETB', timezone: 'Africa/Addis_Ababa' }
  });

  const createMutation = useMutation({
    mutationFn: (values: FormValues) => createOrganization(values as CreateOrganizationRequest),
    onSuccess: () => {
      toast.success('Organization created.');
      setShowCreate(false);
      reset();
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
    },
    onError: (error) => toast.error(getErrorMessage(error))
  });

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Organizations</h1>
        <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
          New Organization
        </button>
      </div>

      <div className="card">
        <div className="table-responsive">
          <table className="table table-hover mb-0 align-middle">
            <thead>
              <tr>
                <th>Name</th>
                <th>Code</th>
                <th>Properties</th>
                <th>Currency</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {isLoading && (
                <tr>
                  <td colSpan={5} className="text-center text-muted py-4">
                    Loading…
                  </td>
                </tr>
              )}
              {!isLoading && data?.items.length === 0 && (
                <tr>
                  <td colSpan={5} className="text-center text-muted py-4">
                    No organizations yet.
                  </td>
                </tr>
              )}
              {data?.items.map((org) => (
                <tr key={org.id}>
                  <td>{org.name}</td>
                  <td>
                    <code>{org.code}</code>
                  </td>
                  <td>{org.propertyCount}</td>
                  <td>{org.currency}</td>
                  <td>
                    <span className={`badge ${org.status === 'Active' ? 'text-bg-success' : 'text-bg-secondary'}`}>
                      {org.status}
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
        <Modal title="New Organization" onClose={() => setShowCreate(false)}>
          <form onSubmit={handleSubmit((values) => createMutation.mutate(values))} noValidate>
            <div className="mb-3">
              <label className="form-label">Name</label>
              <input className={`form-control ${errors.name ? 'is-invalid' : ''}`} {...register('name')} />
              {errors.name && <div className="invalid-feedback">{errors.name.message}</div>}
            </div>
            <div className="mb-3">
              <label className="form-label">Code</label>
              <input className={`form-control ${errors.code ? 'is-invalid' : ''}`} {...register('code')} />
              {errors.code && <div className="invalid-feedback">{errors.code.message}</div>}
            </div>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input className={`form-control ${errors.email ? 'is-invalid' : ''}`} {...register('email')} />
              {errors.email && <div className="invalid-feedback">{errors.email.message}</div>}
            </div>
            <div className="row">
              <div className="col mb-3">
                <label className="form-label">Currency</label>
                <input className={`form-control ${errors.currency ? 'is-invalid' : ''}`} {...register('currency')} />
              </div>
              <div className="col mb-3">
                <label className="form-label">Timezone</label>
                <input className={`form-control ${errors.timezone ? 'is-invalid' : ''}`} {...register('timezone')} />
              </div>
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
