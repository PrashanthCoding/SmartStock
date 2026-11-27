import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { Inventory, InventorySummary, LowStockItem } from '../../core/models/inventory';

import { InventoryService } from '../../core/services/inventory.service';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe],
  templateUrl: './inventories.html',
  styleUrl: './inventories.css',
})
export class Inventories implements OnInit {
  inventories: Inventory[] = [];
  lowStockItems: LowStockItem[] = [];

  summary: InventorySummary | null = null;

  loading = false;
  errorMessage = '';
  successMessage = '';

  search = '';
  stockFilter = '';

  constructor(private inventoryService: InventoryService) {}

  ngOnInit(): void {
    this.loadInventory();
  }

  loadInventory(): void {
    this.loading = true;
    this.errorMessage = '';

    forkJoin({
      inventories: this.inventoryService.getInventories(),
      lowStock: this.inventoryService.getLowStock(),
      summary: this.inventoryService.getSummary(),
    }).subscribe({
      next: (result) => {
        this.inventories = result.inventories;
        this.lowStockItems = result.lowStock;
        this.summary = result.summary;

        this.loading = false;

        console.log('Inventory:', result.inventories);
        console.log('Low Stock:', result.lowStock);
        console.log('Summary:', result.summary);
      },
      error: (error) => {
        console.error('Inventory API Error:', error);

        this.errorMessage = error.error?.message || 'Unable to load inventory data.';

        this.loading = false;
      },
    });
  }

  get filteredInventories(): Inventory[] {
    return this.inventories.filter((item) => {
      const matchesSearch =
        !this.search || item.productName.toLowerCase().includes(this.search.toLowerCase());

      const isLowStock = this.isLowStock(item);

      const matchesFilter =
        !this.stockFilter ||
        (this.stockFilter === 'low' && isLowStock && item.quantity > 0) ||
        (this.stockFilter === 'out' && item.quantity === 0) ||
        (this.stockFilter === 'available' && !isLowStock);

      return matchesSearch && matchesFilter;
    });
  }

  isLowStock(item: Inventory): boolean {
    return this.lowStockItems.some((low) => low.inventoryId === item.id);
  }

  getStockStatus(item: Inventory): string {
    if (item.quantity === 0) {
      return 'Out of Stock';
    }

    if (this.isLowStock(item)) {
      return 'Low Stock';
    }

    return 'In Stock';
  }

  getStatusClass(item: Inventory): string {
    if (item.quantity === 0) {
      return 'out';
    }

    if (this.isLowStock(item)) {
      return 'low';
    }

    return 'available';
  }

  clearFilters(): void {
    this.search = '';
    this.stockFilter = '';
  }

  refresh(): void {
    this.loadInventory();
  }

  trackByInventoryId(index: number, item: Inventory): number {
    return item.id;
  }
}
