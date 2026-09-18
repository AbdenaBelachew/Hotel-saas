export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: ApiError[];
}

export interface ApiError {
  field: string | null;
  message: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PagedQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export type OrganizationStatus = 'Active' | 'Suspended' | 'Inactive';
export type PropertyStatus = 'Active' | 'Inactive';
export type PropertyType =
  | 'Hotel'
  | 'Resort'
  | 'GuestHouse'
  | 'Apartment'
  | 'Lodge'
  | 'Villa'
  | 'Hostel'
  | 'ServicedApartment';

export interface Organization {
  id: string;
  name: string;
  legalName: string | null;
  code: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  city: string | null;
  country: string | null;
  logoUrl: string | null;
  currency: string;
  timezone: string;
  status: OrganizationStatus;
  propertyCount: number;
  createdAt: string;
}

export interface Property {
  id: string;
  organizationId: string;
  name: string;
  code: string;
  propertyType: PropertyType;
  description: string | null;
  address: string | null;
  city: string | null;
  country: string | null;
  phone: string | null;
  email: string | null;
  website: string | null;
  logoUrl: string | null;
  currency: string;
  timezone: string;
  checkInTime: string;
  checkOutTime: string;
  status: PropertyStatus;
  createdAt: string;
}

export interface AppUser {
  id: string;
  organizationId: string | null;
  username: string;
  email: string;
  firstName: string;
  lastName: string | null;
  phone: string | null;
  isActive: boolean;
  roles: string[];
  propertyIds: string[];
  createdAt: string;
}

export interface UserProfile {
  id: string;
  organizationId: string | null;
  organizationName: string | null;
  username: string;
  email: string;
  firstName: string;
  lastName: string | null;
  isActive: boolean;
  roles: string[];
  permissions: string[];
  propertyIds: string[];
}

export interface AuthResult {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: UserProfile;
}

export interface Role {
  id: string;
  name: string;
  description: string | null;
  isSystemRole: boolean;
  permissions: string[];
}
