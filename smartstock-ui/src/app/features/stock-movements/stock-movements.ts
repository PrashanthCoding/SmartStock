import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

const API_URL = 'https://localhost:7101/api';

enum MovementType {
  StockIn = 1,
  StockOut = 2,
  Adjustment = 3,
}

interface InventoryItem {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  rowVersion: string;
}

interface StockMovement {
  id: number;
  inventoryId: number;
  productId: number;
  productName: string;
  movementType: number;
  quantity: number;
  quantityBefore: number;
  quantityAfter: number;
  remarks: string | null;
  createdAtUtc: string;
}

interface CreateMovement {
  productId: number;
  movementType: number;
  quantity: number;
  rowVersion: string;
  remarks: string;
}

@Component({
  selector: 'app-stock-movements',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock-movements.html',
  styleUrls: ['./stock-movements.css'],
})
export class StockMovements implements OnInit {
  private http = inject(HttpClient);

  movements: StockMovement[] = [];
  inventories: InventoryItem[] = [];

  isLoading = false;
  isSaving = false;
  showModal = false;

  errorMessage = '';
  successMessage = '';

  searchText = '';
  selectedFilter = 'all';

  movementForm: CreateMovement = this.emptyForm();

  private emptyForm(): CreateMovement {
    return {
      productId: 0,
      movementType: MovementType.StockIn,
      quantity: 0,
      rowVersion: '',
      remarks: '',
    };
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      movements: this.http.get<StockMovement[]>(`${API_URL}/InventoryMovements`),
      inventories: this.http.get<InventoryItem[]>(`${API_URL}/Inventories`),
    }).subscribe({
      next: (result) => {
        this.movements = result.movements ?? [];
        this.inventories = result.inventories ?? [];
        this.isLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading = false;
        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  get totalMovements(): number {
    return this.movements.length;
  }

  get totalStockIn(): number {
    return this.movements
      .filter((x) => x.movementType === MovementType.StockIn)
      .reduce((sum, x) => sum + x.quantity, 0);
  }

  get totalStockOut(): number {
    return this.movements
      .filter((x) => x.movementType === MovementType.StockOut)
      .reduce((sum, x) => sum + x.quantity, 0);
  }

  get totalAdjustments(): number {
    return this.movements.filter((x) => x.movementType === MovementType.Adjustment).length;
  }

  get filteredMovements(): StockMovement[] {
    const search = this.searchText.trim().toLowerCase();

    return this.movements.filter((item) => {
      const matchesSearch =
        !search ||
        (item.productName ?? '').toLowerCase().includes(search) ||
        item.productId.toString().includes(search) ||
        (item.remarks ?? '').toLowerCase().includes(search);

      const matchesType =
        this.selectedFilter === 'all' || item.movementType.toString() === this.selectedFilter;

      return matchesSearch && matchesType;
    });
  }

  get selectedInventory(): InventoryItem | undefined {
    return this.inventories.find((x) => x.productId === Number(this.movementForm.productId));
  }

  getMovementName(type: number): string {
    switch (type) {
      case MovementType.StockIn:
        return 'Stock In';
      case MovementType.StockOut:
        return 'Stock Out';
      case MovementType.Adjustment:
        return 'Adjustment';
      default:
        return 'Unknown';
    }
  }

  getMovementClass(type: number): string {
    switch (type) {
      case MovementType.StockIn:
        return 'stock-in';
      case MovementType.StockOut:
        return 'stock-out';
      case MovementType.Adjustment:
        return 'adjustment';
      default:
        return '';
    }
  }

  get quantityLabel(): string {
    return this.movementForm.movementType === MovementType.Adjustment
      ? 'Actual Physical Stock'
      : 'Movement Quantity';
  }

  openAddModal(): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.movementForm = this.emptyForm();
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.movementForm = this.emptyForm();
  }

  saveMovement(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const inventory = this.selectedInventory;
    const quantity = Number(this.movementForm.quantity);

    if (!inventory) {
      this.errorMessage = 'Please select a product.';
      return;
    }

    if (!Number.isFinite(quantity) || quantity <= 0) {
      this.errorMessage = 'Enter a valid quantity greater than zero.';
      return;
    }

    if (!inventory.rowVersion) {
      this.errorMessage = 'RowVersion is missing. Update GET api/Inventories to return RowVersion.';
      return;
    }

    if (this.movementForm.movementType === MovementType.StockOut && quantity > inventory.quantity) {
      this.errorMessage = 'Insufficient available stock.';
      return;
    }

    if (
      this.movementForm.movementType === MovementType.Adjustment &&
      quantity === inventory.quantity
    ) {
      this.errorMessage = 'Physical stock is already equal to current stock.';
      return;
    }

    const payload: CreateMovement = {
      productId: inventory.productId,
      movementType: Number(this.movementForm.movementType),
      quantity,
      rowVersion: inventory.rowVersion,
      remarks: this.movementForm.remarks.trim(),
    };

    this.isSaving = true;

    this.http.post(`${API_URL}/InventoryMovements`, payload).subscribe({
      next: () => {
        this.isSaving = false;
        this.showModal = false;
        this.successMessage = 'Stock movement created successfully.';
        this.movementForm = this.emptyForm();
        this.loadData();
      },
      error: (error: HttpErrorResponse) => {
        this.isSaving = false;

        if (error.status === 409) {
          this.errorMessage = 'Inventory changed. Refresh the data and try again.';
          this.loadData();
          return;
        }

        this.errorMessage = this.getErrorMessage(error);
      },
    });
  }

  refreshData(): void {
    this.loadData();
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Cannot connect to API. Check the backend and API URL.';
    }

    if (error.status === 401) {
      return 'Session expired. Please login again.';
    }

    if (error.status === 403) {
      return 'You do not have permission to perform this action.';
    }

    return error.error?.message || error.error?.title || 'Something went wrong. Please try again.';
  }
}
