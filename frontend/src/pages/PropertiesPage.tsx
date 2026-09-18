import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { toast } from 'react-toastify';
import { createProperty, getProperties, type CreatePropertyRequest } from '../api/properties';
import { getErrorMessage } from '../api/client';
import { Modal } from '../components/Modal';
import { useAuthStore } from '../stores/authStore';

const PROPERTY_TYPES = ['Hotel', 'Resort', 'GuestHouse', 'Apartment', 'Lodge', 'Villa', 'Hostel', 'ServicedApartment'] as const;

const schema = z.object({
  name: z.string().min(1, 'Name is required.'),
  code: z
    .string()
    .min(1, 'Code is required.')
    .regex(/^[A-Za-z0-9-]+$/, 'Letters, digits and hyphens only.'),
  propertyType: z.enum(PROPERTY_TYPES),
  currency: z.string().min(3).max(3),
  timezone: z.string().min(1),
  checkInTime: z.string().min(1),
  checkOutTime: z.string().min(1)
});

type FormValues = z.infer<typeof schema>;

export function PropertiesPage() {
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);
  const queryClient = useQueryClient();
  const hasPermission = useAuthStore((s) => s.hasPermission);

  const { data, isLoading } = useQuery({
    queryKey: ['properties', page],
    queryFn: () => getProperties({ page, pageSize: 10 })
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      propertyType: 'Hotel',
      currency: 'ETB',
      timezone: 'Africa/Addis_Ababa',
      checkInTime: '14:00',
      checkOutTime: '12:00'
    }
  });

  const createMutation = useMutation({
    mutationFn: (values: FormValues) =>
      createProperty({ ...values, checkInTime: `${values.checkInTime}:00`, checkOutTime: `${values.checkOutTime}:00` } as CreatePropertyRequest),
    onSuccess: () => {
      toast.success('Property created.');
      setShowCreate(false);
      reset();
      queryClient.invalidateQueries({ queryKey: ['properties'] });
    },
    onError: (error) => toast.error(getErrorMessage(error))
  });

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Properties</h1>
        {hasPermission('properties.create') && (
          <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
            New Property
          </button>
        )}
      </div>

      <div className="card">
        <div className="table-responsive">
          <table className="table table-hover mb-0 align-middle">
            <thead>
              <tr>
                <th>Name</th>
                <th>Code</th>
                <th>Type</th>
                <th>Check-in / Check-out</th>
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
                    No properties yet.
                  </td>
                </tr>
              )}
              {data?.items.map((property) => (
                <tr key={property.id}>
                  <td>{property.name}</td>
                  <td>
                    <code>{property.code}</code>
                  </td>
                  <td>{property.propertyType}</td>
                  <td>
                    {property.checkInTime.slice(0, 5)} / {property.checkOutTime.slice(0, 5)}
                  </td>
                  <td>
                    <span className={`badge ${property.status === 'Active' ? 'text-bg-success' : 'text-bg-secondary'}`}>
                      {property.status}
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
        <Modal title="New Property" onClose={() => setShowCreate(false)}>
          <form onSubmit={handleSubmit((values) => createMutation.mutate(values))} noValidate>
            <div className="mb-3">
              <label className="form-label">Name</label>
              <input className={`form-control ${errors.name ? 'is-invalid' : ''}`} {...register('name')} />
              {errors.name && <div className="invalid-feedback">{errors.name.message}</div>}
            </div>
            <div className="row">
              <div className="col mb-3">
                <label className="form-label">Code</label>
                <input className={`form-control ${errors.code ? 'is-invalid' : ''}`} {...register('code')} />
                {errors.code && <div className="invalid-feedback">{errors.code.message}</div>}
              </div>
              <div className="col mb-3">
                <label className="form-label">Type</label>
                <select className="form-select" {...register('propertyType')}>
                  {PROPERTY_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="row">
              <div className="col mb-3">
                <label className="form-label">Currency</label>
                <input className="form-control" {...register('currency')} />
              </div>
              <div className="col mb-3">
                <label className="form-label">Timezone</label>
                <input className="form-control" {...register('timezone')} />
              </div>
            </div>
            <div className="row">
              <div className="col mb-3">
                <label className="form-label">Check-in time</label>
                <input type="time" className="form-control" {...register('checkInTime')} />
              </div>
              <div className="col mb-3">
                <label className="form-label">Check-out time</label>
                <input type="time" className="form-control" {...register('checkOutTime')} />
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
