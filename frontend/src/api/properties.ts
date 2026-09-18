import { apiClient } from './client';
import type { ApiResponse, PagedQuery, PagedResult, Property, PropertyStatus, PropertyType } from '../types';

export interface CreatePropertyRequest {
  organizationId?: string;
  name: string;
  code: string;
  propertyType: PropertyType;
  description?: string;
  address?: string;
  city?: string;
  country?: string;
  phone?: string;
  email?: string;
  website?: string;
  currency: string;
  timezone: string;
  checkInTime: string;
  checkOutTime: string;
}

export interface UpdatePropertyRequest {
  name: string;
  propertyType: PropertyType;
  description?: string;
  address?: string;
  city?: string;
  country?: string;
  phone?: string;
  email?: string;
  website?: string;
  currency: string;
  timezone: string;
  checkInTime: string;
  checkOutTime: string;
  status: PropertyStatus;
}

export async function getProperties(query: PagedQuery): Promise<PagedResult<Property>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<Property>>>('/properties', { params: query });
  return data.data!;
}

export async function createProperty(request: CreatePropertyRequest): Promise<Property> {
  const { data } = await apiClient.post<ApiResponse<Property>>('/properties', request);
  return data.data!;
}

export async function updateProperty(id: string, request: UpdatePropertyRequest): Promise<Property> {
  const { data } = await apiClient.put<ApiResponse<Property>>(`/properties/${id}`, request);
  return data.data!;
}

export async function deleteProperty(id: string): Promise<void> {
  await apiClient.delete(`/properties/${id}`);
}
