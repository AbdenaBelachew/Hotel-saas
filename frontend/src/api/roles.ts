import { apiClient } from './client';
import type { ApiResponse, Role } from '../types';

export async function getRoles(): Promise<Role[]> {
  const { data } = await apiClient.get<ApiResponse<Role[]>>('/roles');
  return data.data!;
}
