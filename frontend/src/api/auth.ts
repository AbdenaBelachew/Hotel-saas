import { apiClient } from './client';
import type { ApiResponse, AuthResult, UserProfile } from '../types';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  organizationName: string;
  organizationCode: string;
  email: string;
  username: string;
  password: string;
  firstName: string;
  lastName?: string;
}

export async function login(request: LoginRequest): Promise<AuthResult> {
  const { data } = await apiClient.post<ApiResponse<AuthResult>>('/auth/login', request);
  return data.data!;
}

export async function register(request: RegisterRequest): Promise<AuthResult> {
  const { data } = await apiClient.post<ApiResponse<AuthResult>>('/auth/register', request);
  return data.data!;
}

export async function logout(refreshToken: string): Promise<void> {
  await apiClient.post('/auth/logout', { refreshToken });
}

export async function fetchProfile(): Promise<UserProfile> {
  const { data } = await apiClient.get<ApiResponse<UserProfile>>('/auth/me');
  return data.data!;
}

export async function changePassword(currentPassword: string, newPassword: string): Promise<void> {
  await apiClient.post('/auth/change-password', { currentPassword, newPassword });
}
