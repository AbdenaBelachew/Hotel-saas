import { apiClient } from './client';
import type { ApiResponse, AppUser, PagedQuery, PagedResult } from '../types';

export interface CreateUserRequest {
  organizationId?: string;
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName?: string;
  phone?: string;
  roleName: string;
  propertyIds?: string[];
}

export interface UpdateUserRequest {
  firstName: string;
  lastName?: string;
  phone?: string;
  isActive: boolean;
  roleName: string;
  propertyIds?: string[];
}

export async function getUsers(query: PagedQuery): Promise<PagedResult<AppUser>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<AppUser>>>('/users', { params: query });
  return data.data!;
}

export async function createUser(request: CreateUserRequest): Promise<AppUser> {
  const { data } = await apiClient.post<ApiResponse<AppUser>>('/users', request);
  return data.data!;
}

export async function updateUser(id: string, request: UpdateUserRequest): Promise<AppUser> {
  const { data } = await apiClient.put<ApiResponse<AppUser>>(`/users/${id}`, request);
  return data.data!;
}

export async function deleteUser(id: string): Promise<void> {
  await apiClient.delete(`/users/${id}`);
}
