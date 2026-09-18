import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

interface ProtectedRouteProps {
  children: ReactNode;
  requiredPermission?: string;
  requirePlatformAdmin?: boolean;
}

export function ProtectedRoute({ children, requiredPermission, requirePlatformAdmin }: ProtectedRouteProps) {
  const user = useAuthStore((s) => s.user);
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const isPlatformAdmin = useAuthStore((s) => s.isPlatformAdmin());

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (requirePlatformAdmin && !isPlatformAdmin) {
    return <Navigate to="/dashboard" replace />;
  }

  if (requiredPermission && !hasPermission(requiredPermission)) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}
