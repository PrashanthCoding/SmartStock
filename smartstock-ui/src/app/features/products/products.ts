import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { CreateProduct, Product, ProductSearchRequest } from '../../core/models/product';
import { ProductService } from '../../core/services/product.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products implements OnInit {
  private readonly productService = inject(ProductService);

  products: Product[] = [];
  categories: string[] = [];

  loading = false;
  saving = false;

  errorMessage = '';
  successMessage = '';

  search = '';
  selectedCategory = '';
  selectedStatus = '';

  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  showModal = false;
  isEditMode = false;
  editingProductId: number | null = null;

  showDeleteModal = false;
  deletingProduct: Product | null = null;

  form: CreateProduct = this.emptyForm();

  ngOnInit(): void {
    this.loadProducts();
  }

  emptyForm(): CreateProduct {
    return {
      sku: '',
      productName: '',
      category: '',
      description: '',
      unitPrice: 0,
      reorderLevel: 0,
    };
  }

  get request(): ProductSearchRequest {
    return {
      search: this.search,
      category: this.selectedCategory || undefined,
      isActive: this.selectedStatus === '' ? null : this.selectedStatus === 'true',
      sortBy: 'Name',
      sortDescending: false,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
    };
  }

  loadProducts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService.getProducts(this.request).subscribe({
      next: (response) => {
        this.products = response.items ?? [];
        this.totalCount = response.totalCount ?? 0;
        this.totalPages = response.totalPages ?? 0;

        this.categories = [...new Set(this.products.map((p) => p.category).filter(Boolean))];

        this.loading = false;
      },
      error: (error) => {
        console.error('Product loading failed:', error);
        this.errorMessage = error?.error?.message ?? 'Unable to load products.';
        this.loading = false;
      },
    });
  }

  onSearch(): void {
    this.pageNumber = 1;
    this.loadProducts();
  }

  onFilterChange(): void {
    this.pageNumber = 1;
    this.loadProducts();
  }

  clearFilters(): void {
    this.search = '';
    this.selectedCategory = '';
    this.selectedStatus = '';
    this.pageNumber = 1;
    this.loadProducts();
  }

  previousPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadProducts();
    }
  }

  nextPage(): void {
    if (this.pageNumber < this.totalPages) {
      this.pageNumber++;
      this.loadProducts();
    }
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.editingProductId = null;
    this.form = this.emptyForm();
    this.errorMessage = '';
    this.showModal = true;
  }

  openEditModal(product: Product): void {
    this.isEditMode = true;
    this.editingProductId = product.id;

    this.form = {
      sku: product.sku,
      productName: product.productName,
      category: product.category,
      description: product.description ?? '',
      unitPrice: product.unitPrice,
      reorderLevel: product.reorderLevel,
    };

    this.errorMessage = '';
    this.showModal = true;
  }

  closeModal(): void {
    if (this.saving) return;
    this.showModal = false;
    this.form = this.emptyForm();
  }

  saveProduct(): void {
    if (
      !this.form.sku.trim() ||
      !this.form.productName.trim() ||
      !this.form.category.trim() ||
      this.form.unitPrice <= 0 ||
      this.form.reorderLevel < 0
    ) {
      this.errorMessage = 'Please enter all required fields correctly.';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    const payload: CreateProduct = {
      ...this.form,
      sku: this.form.sku.trim(),
      productName: this.form.productName.trim(),
      category: this.form.category.trim(),
      description: this.form.description?.trim() || null,
    };

    const request$ =
      this.isEditMode && this.editingProductId !== null
        ? this.productService.updateProduct(this.editingProductId, payload)
        : this.productService.createProduct(payload);

    request$.subscribe({
      next: () => {
        this.saving = false;
        this.showModal = false;
        this.successMessage = this.isEditMode
          ? 'Product updated successfully.'
          : 'Product created successfully.';

        this.loadProducts();
      },
      error: (error) => {
        console.error('Product save failed:', error);
        this.errorMessage = error?.error?.message ?? 'Unable to save product.';
        this.saving = false;
      },
    });
  }

  confirmDeactivate(product: Product): void {
    this.deletingProduct = product;
    this.showDeleteModal = true;
  }

  cancelDeactivate(): void {
    this.showDeleteModal = false;
    this.deletingProduct = null;
  }

  deactivateProduct(): void {
    if (!this.deletingProduct) return;

    const id = this.deletingProduct.id;

    this.productService.deactivateProduct(id).subscribe({
      next: () => {
        this.showDeleteModal = false;
        this.deletingProduct = null;
        this.successMessage = 'Product deactivated successfully.';

        if (this.products.length === 1 && this.pageNumber > 1) {
          this.pageNumber--;
        }

        this.loadProducts();
      },
      error: (error) => {
        console.error('Product deactivation failed:', error);
        this.errorMessage = error?.error?.message ?? 'Unable to deactivate product.';
        this.showDeleteModal = false;
      },
    });
  }

  refresh(): void {
    this.loadProducts();
  }

  getStatusClass(isActive: boolean): string {
    return isActive ? 'status-active' : 'status-inactive';
  }
}
