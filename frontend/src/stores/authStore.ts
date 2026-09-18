import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthResult, UserProfile } from '../types';

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: UserProfile | null;
  setAuth: (result: AuthResult) => void;
  updateUser: (user: UserProfile) => void;
  clear: () => void;
  hasPermission: (permission: string) => boolean;
  isPlatformAdmin: () => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      setAuth: (result) =>
        set({
          accessToken: result.accessToken,
          refreshToken: result.refreshToken,
          user: result.user
        }),
      updateUser: (user) => set({ user }),
      clear: () => set({ accessToken: null, refreshToken: null, user: null }),
      hasPermission: (permission) => {
        const user = get().user;
        if (!user) return false;
        return user.roles.includes('PlatformAdmin') || user.permissions.includes(permission);
      },
      isPlatformAdmin: () => get().user?.roles.includes('PlatformAdmin') ?? false
    }),
    { name: 'pms-auth' }
  )
);
