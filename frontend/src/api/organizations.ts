import { apiClient } from './client';
import type { ApiResponse, Organization, OrganizationStatus, PagedQuery, PagedResult } from '../types';

export interface CreateOrganizationRequest {
  name: string;
  legalName?: string;
  code: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  currency: string;
  timezone: string;
}

export interface UpdateOrganizationRequest {
  name: string;
  legalName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  currency: string;
  timezone: string;
  status: OrganizationStatus;
}

export async function getOrganizations(query: PagedQuery): Promise<PagedResult<Organization>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<Organization>>>('/organizations', { params: query });
  return data.data!;
}

export async function createOrganization(request: CreateOrganizationRequest): Promise<Organization> {
  const { data } = await apiClient.post<ApiResponse<Organization>>('/organizations', request);
  return data.data!;
}

export async function updateOrganization(id: string, request: UpdateOrganizationRequest): Promise<Organization> {
  const { data } = await apiClient.put<ApiResponse<Organization>>(`/organizations/${id}`, request);
  return data.data!;
}

export async function deleteOrganization(id: string): Promise<void> {
  await apiClient.delete(`/organizations/${id}`);
}
