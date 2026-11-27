import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';

import {
  InventoryMovement,
  InventorySummary,
  LowStockItem,
  RecentCustomerOrder,
} from '../../core/models/dashboard';

import { DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private dashboardService = inject(DashboardService);

  loading = true;
  errorMessage = '';

  inventorySummary: InventorySummary | null = null;
  lowStockItems: LowStockItem[] = [];
  movements: InventoryMovement[] = [];
  recentOrders: RecentCustomerOrder[] = [];

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.errorMessage = '';

    this.dashboardService.getDashboardData().subscribe({
      next: (response) => {
        this.inventorySummary = response.inventorySummary;
        this.lowStockItems = response.lowStock;

        this.movements = [...response.movements]
          .sort((a, b) => new Date(b.createdAtUtc).getTime() - new Date(a.createdAtUtc).getTime())
          .slice(0, 5);

        this.loading = false;
        this.recentOrders = [...response.recentOrders]
          .sort((a, b) => new Date(b.orderDateUtc).getTime() - new Date(a.orderDateUtc).getTime())
          .slice(0, 5);
      },
      error: (error) => {
        console.error('Dashboard API Error:', error);
        this.errorMessage = 'Unable to load dashboard data. Please try again.';
        this.loading = false;
      },
    });
  }

  get stats() {
    const summary = this.inventorySummary;

    return [
      {
        title: 'Total Products',
        value: summary?.totalProducts ?? 0,
        icon: '▣',
        color: 'blue',
        change: 'Products with inventory records',
      },
      {
        title: 'Total Stock',
        value: summary?.totalQuantity ?? 0,
        icon: '◫',
        color: 'green',
        change: 'Units currently available',
      },
      {
        title: 'Total Customers',
        value: summary?.totalCustomers ?? 0,
        icon: '♙',
        color: 'purple',
        change: 'Active registered customers',
      },
      {
        title: 'Total Orders',
        value: summary?.totalOrders ?? 0,
        icon: '▤',
        color: 'orange',
        change: 'Total orders received',
      },
    ];
  }

  get stockHealthPercentage(): number {
    const total = this.inventorySummary?.totalProducts ?? 0;
    const lowStock = this.inventorySummary?.lowStockCount ?? 0;

    if (total === 0) return 0;

    return Math.max(0, Math.min(100, ((total - lowStock) / total) * 100));
  }

  get totalInventoryValue(): number {
    return this.inventorySummary?.totalInventoryValue ?? 0;
  }

  getMovementType(type: number): string {
    switch (type) {
      case 1:
        return 'Stock In';
      case 2:
        return 'Stock Out';
      case 3:
        return 'Adjustment';
      default:
        return 'Unknown';
    }
  }
}
