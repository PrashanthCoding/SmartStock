import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';

const API_URL = 'https://localhost:7101/api/Customers';

// Actual GET API response
interface Customer {
  id: number;
  customerName: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

// Form model
interface CustomerForm {
  customerName: string;
  email: string;
  phone: string;
  address: string;
  isActive: boolean;
}

// POST request
interface CreateCustomerRequest {
  customerName: string;
  email: string | null;
  phone: string | null;
  address: string | null;
}

// PUT request
interface UpdateCustomerRequest extends CreateCustomerRequest {
  isActive: boolean;
}

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customers.html',
  styleUrl: './customers.css',
})
export class Customers implements OnInit {
  private http = inject(HttpClient);

  // Database data
  customers: Customer[] = [];

  // Loading states
  isLoading = false;
  isSaving = false;

  // Modal
  showModal = false;
  isEditMode = false;
  selectedCustomerId: number | null = null;

  // Messages
  errorMessage = '';
  successMessage = '';

  // Search and filter
  searchText = '';
  selectedStatus = 'all';

  // Form
  customerForm: CustomerForm = this.emptyForm();

  private emptyForm(): CustomerForm {
    return {
      customerName: '',
      email: '',
      phone: '',
      address: '',
      isActive: true,
    };
  }

  // =========================================
  // INITIALIZE
  // =========================================

  ngOnInit(): void {
    this.loadCustomers();
  }

  // =========================================
  // GET ALL CUSTOMERS
  // GET: api/Customers
  // =========================================

  loadCustomers(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.http.get<Customer[]>(API_URL).subscribe({
      next: (response) => {
        this.customers = response ?? [];
        this.isLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading = false;
        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  // =========================================
  // SUMMARY
  // =========================================

  get totalCustomers(): number {
    return this.customers.length;
  }

  get activeCustomers(): number {
    return this.customers.filter((x) => x.isActive).length;
  }

  get inactiveCustomers(): number {
    return this.customers.filter((x) => !x.isActive).length;
  }

  // =========================================
  // SEARCH AND FILTER
  // =========================================

  get filteredCustomers(): Customer[] {
    const search = this.searchText.trim().toLowerCase();

    return this.customers.filter((customer) => {
      const matchesSearch =
        !search ||
        (customer.customerName ?? '').toLowerCase().includes(search) ||
        (customer.email ?? '').toLowerCase().includes(search) ||
        (customer.phone ?? '').toLowerCase().includes(search) ||
        (customer.address ?? '').toLowerCase().includes(search) ||
        customer.id.toString().includes(search);

      const matchesStatus =
        this.selectedStatus === 'all' ||
        (this.selectedStatus === 'active' && customer.isActive) ||
        (this.selectedStatus === 'inactive' && !customer.isActive);

      return matchesSearch && matchesStatus;
    });
  }

  // =========================================
  // OPEN ADD CUSTOMER MODAL
  // =========================================

  openAddModal(): void {
    this.isEditMode = false;
    this.selectedCustomerId = null;
    this.customerForm = this.emptyForm();

    this.errorMessage = '';
    this.showModal = true;
  }

  // =========================================
  // GET CUSTOMER BY ID
  // GET: api/Customers/{id}
  // =========================================

  editCustomer(customer: Customer): void {
    this.errorMessage = '';
    this.successMessage = '';

    this.http.get<Customer>(`${API_URL}/${customer.id}`).subscribe({
      next: (response) => {
        this.isEditMode = true;
        this.selectedCustomerId = response.id;

        this.customerForm = {
          customerName: response.customerName ?? '',
          email: response.email ?? '',
          phone: response.phone ?? '',
          address: response.address ?? '',
          isActive: response.isActive,
        };

        this.showModal = true;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  // =========================================
  // CLOSE MODAL
  // =========================================

  closeModal(): void {
    this.showModal = false;
    this.customerForm = this.emptyForm();
    this.selectedCustomerId = null;
    this.errorMessage = '';
  }

  // =========================================
  // CREATE / UPDATE CUSTOMER
  // =========================================

  saveCustomer(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const customerName = this.customerForm.customerName.trim();

    if (!customerName) {
      this.errorMessage = 'Customer name is required.';
      return;
    }

    const email = this.customerForm.email.trim();

    if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      this.errorMessage = 'Enter a valid email address.';
      return;
    }

    const commonPayload: CreateCustomerRequest = {
      customerName,
      email: email || null,
      phone: this.customerForm.phone.trim() || null,
      address: this.customerForm.address.trim() || null,
    };

    this.isSaving = true;

    if (this.isEditMode && this.selectedCustomerId !== null) {
      // PUT: api/Customers/{id}
      const payload: UpdateCustomerRequest = {
        ...commonPayload,
        isActive: this.customerForm.isActive,
      };

      this.http.put(`${API_URL}/${this.selectedCustomerId}`, payload).subscribe({
        next: () => {
          this.isSaving = false;
          this.showModal = false;
          this.successMessage = 'Customer updated successfully.';
          this.loadCustomers();
        },
        error: (error: HttpErrorResponse) => {
          this.isSaving = false;
          this.errorMessage = this.getErrorMessage(error);
        },
      });
    } else {
      // POST: api/Customers
      this.http.post(API_URL, commonPayload).subscribe({
        next: () => {
          this.isSaving = false;
          this.showModal = false;
          this.successMessage = 'Customer created successfully.';
          this.loadCustomers();
        },
        error: (error: HttpErrorResponse) => {
          this.isSaving = false;
          this.errorMessage = this.getErrorMessage(error);
        },
      });
    }
  }

  // =========================================
  // DELETE / DEACTIVATE CUSTOMER
  // DELETE: api/Customers/{id}
  // =========================================

  deleteCustomer(customer: Customer): void {
    const confirmed = confirm(`Are you sure you want to deactivate ${customer.customerName}?`);

    if (!confirmed) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';

    this.http.delete(`${API_URL}/${customer.id}`).subscribe({
      next: () => {
        this.successMessage = 'Customer deactivated successfully.';
        this.loadCustomers();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  // =========================================
  // CUSTOMER STATUS
  // =========================================

  getCustomerStatus(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  getInitials(name: string): string {
    return (name ?? '')
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((x) => x.charAt(0).toUpperCase())
      .join('');
  }

  // =========================================
  // REFRESH
  // =========================================

  refreshCustomers(): void {
    this.loadCustomers();
  }

  // =========================================
  // ERROR HANDLING
  // =========================================

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the API. Check the backend and API URL.';
    }

    if (error.status === 400) {
      return error.error?.message || 'Invalid customer details.';
    }

    if (error.status === 401) {
      return 'Session expired. Please login again.';
    }

    if (error.status === 403) {
      return 'You do not have permission to perform this action.';
    }

    if (error.status === 404) {
      return error.error?.message || 'Customer not found.';
    }

    if (error.status === 409) {
      return error.error?.message || 'Customer email already exists.';
    }

    return error.error?.message || error.error?.title || 'Something went wrong. Please try again.';
  }
}
