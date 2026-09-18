import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '../stores/authStore';
import { getProperties } from '../api/properties';
import { getUsers } from '../api/users';
import { getOrganizations } from '../api/organizations';

function StatCard({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="col-sm-6 col-lg-3">
      <div className="card stat-card h-100">
        <div className="card-body">
          <div className="text-muted small text-uppercase">{label}</div>
          <div className="fs-2 fw-semibold">{value}</div>
        </div>
      </div>
    </div>
  );
}

export function DashboardPage() {
  const user = useAuthStore((s) => s.user);
  const isPlatformAdmin = useAuthStore((s) => s.isPlatformAdmin());
  const hasPermission = useAuthStore((s) => s.hasPermission);

  const propertiesQuery = useQuery({
    queryKey: ['dashboard', 'properties'],
    queryFn: () => getProperties({ page: 1, pageSize: 1 }),
    enabled: hasPermission('properties.view')
  });

  const usersQuery = useQuery({
    queryKey: ['dashboard', 'users'],
    queryFn: () => getUsers({ page: 1, pageSize: 1 }),
    enabled: hasPermission('users.view')
  });

  const organizationsQuery = useQuery({
    queryKey: ['dashboard', 'organizations'],
    queryFn: () => getOrganizations({ page: 1, pageSize: 1 }),
    enabled: isPlatformAdmin
  });

  return (
    <div>
      <h1 className="h4 mb-1">Welcome, {user?.firstName}</h1>
      <p className="text-muted mb-4">
        {user?.organizationName ?? 'Platform administration'} &middot; {user?.roles.join(', ')}
      </p>

      <div className="row g-3">
        {isPlatformAdmin && <StatCard label="Organizations" value={organizationsQuery.data?.totalCount ?? '—'} />}
        {hasPermission('properties.view') && (
          <StatCard label="Properties" value={propertiesQuery.data?.totalCount ?? '—'} />
        )}
        {hasPermission('users.view') && <StatCard label="Users" value={usersQuery.data?.totalCount ?? '—'} />}
      </div>

      <div className="alert alert-info mt-4 mb-0">
        This is the Phase 1 foundation: authentication, organizations, properties and user/role
        management. Rooms, reservations, payments and the rest of the PMS modules land in later
        phases.
      </div>
    </div>
  );
}
