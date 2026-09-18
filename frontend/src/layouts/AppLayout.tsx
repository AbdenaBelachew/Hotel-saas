import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import { logout as logoutRequest } from '../api/auth';

export function AppLayout() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const refreshToken = useAuthStore((s) => s.refreshToken);
  const isPlatformAdmin = useAuthStore((s) => s.isPlatformAdmin());
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const clear = useAuthStore((s) => s.clear);

  const handleLogout = async () => {
    if (refreshToken) {
      try {
        await logoutRequest(refreshToken);
      } catch {
        // best-effort: still clear local session
      }
    }
    clear();
    navigate('/login', { replace: true });
  };

  return (
    <div className="d-flex app-shell">
      <aside className="sidebar d-flex flex-column p-3">
        <div className="brand-mark text-white fs-4 mb-4">Hotel PMS</div>
        <nav className="nav nav-pills flex-column gap-1">
          <NavLink to="/dashboard" className="nav-link">
            Dashboard
          </NavLink>
          {isPlatformAdmin && (
            <NavLink to="/organizations" className="nav-link">
              Organizations
            </NavLink>
          )}
          {hasPermission('properties.view') && (
            <NavLink to="/properties" className="nav-link">
              Properties
            </NavLink>
          )}
          {hasPermission('users.view') && (
            <NavLink to="/users" className="nav-link">
              Users
            </NavLink>
          )}
        </nav>
      </aside>

      <div className="flex-grow-1 d-flex flex-column">
        <header className="topbar d-flex align-items-center justify-content-between px-4 py-2">
          <div className="text-muted small">
            {user?.organizationName ?? 'Platform Administration'}
          </div>
          <div className="d-flex align-items-center gap-3">
            <span className="small">
              {user?.firstName} {user?.lastName} &middot; <span className="text-muted">{user?.roles.join(', ')}</span>
            </span>
            <button className="btn btn-outline-secondary btn-sm" onClick={handleLogout}>
              Log out
            </button>
          </div>
        </header>

        <main className="flex-grow-1 p-4">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
