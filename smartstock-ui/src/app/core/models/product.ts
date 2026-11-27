export interface Product {
  id: number;
  sku: string;
  productName: string;
  category: string;
  description: string | null;
  unitPrice: number;
  reorderLevel: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface CreateProduct {
  sku: string;
  productName: string;
  category: string;
  description: string | null;
  unitPrice: number;
  reorderLevel: number;
}

export interface UpdateProduct extends CreateProduct {}

export interface ProductSearchRequest {
  search?: string;
  category?: string;
  isActive?: boolean | null;
  sortBy: string;
  sortDescending: boolean;
  pageNumber: number;
  pageSize: number;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
