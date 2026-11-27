import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  CreateProduct,
  PagedResponse,
  Product,
  ProductSearchRequest,
  UpdateProduct,
} from '../models/product';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = 'https://localhost:7101/api/Products';

  getProducts(request: ProductSearchRequest): Observable<PagedResponse<Product>> {
    let params = new HttpParams()
      .set('sortBy', request.sortBy)
      .set('sortDescending', request.sortDescending)
      .set('pageNumber', request.pageNumber)
      .set('pageSize', request.pageSize);

    if (request.search?.trim()) {
      params = params.set('search', request.search.trim());
    }

    if (request.category) {
      params = params.set('category', request.category);
    }

    if (request.isActive !== null && request.isActive !== undefined) {
      params = params.set('isActive', request.isActive);
    }

    return this.http.get<PagedResponse<Product>>(this.apiUrl, { params });
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  createProduct(request: CreateProduct): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, request);
  }

  updateProduct(id: number, request: UpdateProduct): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, request);
  }

  deactivateProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
