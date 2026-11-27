import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';

const API_BASE_URL = 'https://localhost:7101/api';

interface Customer {
  id: number;
  customerName: string;
  isActive: boolean;
}

interface Product {
  id: number;
  sku: string;
  productName: string;
  unitPrice: number;
  isActive: boolean;
}

interface OrderItem {
  id: number;
  productId: number;
  sku: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface CustomerOrder {
  id: number;
  orderNumber: string;
  customerId: number;
  customerName: string;
  orderDateUtc: string;
  status: number | string;
  totalAmount: number;
  items: OrderItem[];
}

interface OrderItemForm {
  productId: number | null;
  quantity: number;
}

interface CreateOrderRequest {
  customerId: number;
  items: {
    productId: number;
    quantity: number;
  }[];
}

@Component({
  selector: 'app-customer-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-orders.html',
  styleUrl: './customer-orders.css',
})
export class CustomerOrders implements OnInit {
  private http = inject(HttpClient);

  private readonly ordersUrl = `${API_BASE_URL}/CustomerOrders`;
  private readonly customersUrl = `${API_BASE_URL}/Customers`;
  private readonly productsUrl = `${API_BASE_URL}/Products`;

  orders: CustomerOrder[] = [];
  customers: Customer[] = [];
  products: Product[] = [];

  isLoading = false;
  isSaving = false;
  isLoadingDetails = false;

  showModal = false;
  showDetailsModal = false;

  searchText = '';
  selectedStatus = 'all';

  errorMessage = '';
  successMessage = '';

  selectedOrder: CustomerOrder | null = null;

  orderForm = {
    customerId: null as number | null,
    items: [{ productId: null, quantity: 1 }] as OrderItemForm[],
  };

  ngOnInit(): void {
    this.loadInitialData();
  }

  loadInitialData(): void {
    this.loadCustomers();
    this.loadProducts();
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.http.get<CustomerOrder[]>(this.ordersUrl).subscribe({
      next: (response) => {
        this.orders = Array.isArray(response) ? response : [];
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = this.getErrorMessage(error);
        this.isLoading = false;
      },
    });
  }

  loadCustomers(): void {
    this.http.get<Customer[]>(this.customersUrl).subscribe({
      next: (response) => {
        this.customers = Array.isArray(response) ? response : [];
      },
      error: (error) => {
        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  loadProducts(): void {
    this.http.get<any>(this.productsUrl).subscribe({
      next: (response) => {
        // Supports a direct array or common paginated response shapes.
        const data = Array.isArray(response)
          ? response
          : (response?.data ?? response?.items ?? response?.products ?? []);

        this.products = Array.isArray(data) ? data : [];
      },
      error: (error) => {
        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  get activeCustomers(): Customer[] {
    return this.customers.filter((x) => x.isActive);
  }

  get activeProducts(): Product[] {
    return this.products.filter((x) => x.isActive);
  }

  get totalOrders(): number {
    return this.orders.length;
  }

  get confirmedOrders(): number {
    return this.orders.filter((x) => this.getStatusText(x.status) === 'Confirmed').length;
  }

  get cancelledOrders(): number {
    return this.orders.filter((x) => this.getStatusText(x.status) === 'Cancelled').length;
  }

  get totalSales(): number {
    return this.orders
      .filter((x) => this.getStatusText(x.status) !== 'Cancelled')
      .reduce((total, order) => total + Number(order.totalAmount || 0), 0);
  }

  get filteredOrders(): CustomerOrder[] {
    const search = this.searchText.trim().toLowerCase();

    return this.orders.filter((order) => {
      const matchesSearch =
        !search ||
        (order.orderNumber ?? '').toLowerCase().includes(search) ||
        (order.customerName ?? '').toLowerCase().includes(search);

      const matchesStatus =
        this.selectedStatus === 'all' ||
        this.getStatusText(order.status).toLowerCase() === this.selectedStatus.toLowerCase();

      return matchesSearch && matchesStatus;
    });
  }

  openCreateModal(): void {
    this.errorMessage = '';
    this.successMessage = '';

    this.orderForm = {
      customerId: null,
      items: [{ productId: null, quantity: 1 }],
    };

    this.showModal = true;
  }

  closeModal(): void {
    if (this.isSaving) return;
    this.showModal = false;
  }

  addItem(): void {
    this.orderForm.items.push({
      productId: null,
      quantity: 1,
    });
  }

  removeItem(index: number): void {
    if (this.orderForm.items.length > 1) {
      this.orderForm.items.splice(index, 1);
    }
  }

  getItemProduct(item: OrderItemForm): Product | undefined {
    return this.products.find((x) => x.id === Number(item.productId));
  }

  getItemTotal(item: OrderItemForm): number {
    const product = this.getItemProduct(item);
    return product ? product.unitPrice * Number(item.quantity || 0) : 0;
  }

  get estimatedTotal(): number {
    return this.orderForm.items.reduce((total, item) => total + this.getItemTotal(item), 0);
  }

  saveOrder(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.orderForm.customerId) {
      this.errorMessage = 'Please select a customer.';
      return;
    }

    if (!this.orderForm.items.length) {
      this.errorMessage = 'Please add at least one product.';
      return;
    }

    const invalidItem = this.orderForm.items.some(
      (item) =>
        !item.productId || !Number.isInteger(Number(item.quantity)) || Number(item.quantity) <= 0,
    );

    if (invalidItem) {
      this.errorMessage = 'Select a product and enter a valid quantity for every item.';
      return;
    }

    const request: CreateOrderRequest = {
      customerId: Number(this.orderForm.customerId),
      items: this.orderForm.items.map((item) => ({
        productId: Number(item.productId),
        quantity: Number(item.quantity),
      })),
    };

    this.isSaving = true;

    this.http.post<CustomerOrder>(this.ordersUrl, request).subscribe({
      next: () => {
        this.isSaving = false;
        this.showModal = false;
        this.successMessage = 'Customer order created successfully.';
        this.loadOrders();
      },
      error: (error) => {
        this.isSaving = false;
        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  viewOrder(order: CustomerOrder): void {
    this.isLoadingDetails = true;
    this.showDetailsModal = true;
    this.selectedOrder = order;

    this.http.get<CustomerOrder>(`${this.ordersUrl}/${order.id}`).subscribe({
      next: (response) => {
        this.selectedOrder = response;
        this.isLoadingDetails = false;
      },
      error: (error) => {
        this.isLoadingDetails = false;
        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  closeDetailsModal(): void {
    this.showDetailsModal = false;
    this.selectedOrder = null;
  }

  cancelOrder(order: CustomerOrder): void {
    if (this.getStatusText(order.status) !== 'Confirmed') {
      return;
    }

    const confirmed = window.confirm(`Are you sure you want to cancel order ${order.orderNumber}?`);

    if (!confirmed) return;

    this.errorMessage = '';
    this.successMessage = '';

    this.http.put(`${this.ordersUrl}/${order.id}/cancel`, {}).subscribe({
      next: () => {
        this.successMessage = `Order ${order.orderNumber} cancelled successfully.`;
        this.loadOrders();

        if (this.selectedOrder?.id === order.id) {
          this.closeDetailsModal();
        }
      },
      error: (error) => {
        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  getStatusText(status: number | string): string {
    if (typeof status === 'string') {
      return status.toLowerCase() === 'confirmed'
        ? 'Confirmed'
        : status.toLowerCase() === 'cancelled'
          ? 'Cancelled'
          : status;
    }

    // Adjust these values if your OrderStatus enum differs.
    switch (status) {
      case 1:
        return 'Confirmed';
      case 2:
        return 'Cancelled';
      default:
        return `Status ${status}`;
    }
  }

  getStatusClass(status: number | string): string {
    return this.getStatusText(status).toLowerCase();
  }

  refreshOrders(): void {
    this.loadOrders();
    this.loadCustomers();
    this.loadProducts();
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    return (
      error.error?.message ||
      error.error?.Message ||
      error.error?.title ||
      (typeof error.error === 'string' ? error.error : null) ||
      error.message ||
      'Something went wrong. Please try again.'
    );
  }
}
