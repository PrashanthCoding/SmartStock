import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

interface OrderStatusSummary {
  status: string;
  count: number;
}

interface MonthlyOrderSummary {
  year: number;
  month: number;
  monthName: string;
  totalOrders: number;
  totalAmount: number;
}

interface LowStockProduct {
  productId: number;
  sku: string;
  productName: string;
  category: string;
  availableQuantity: number;
  reorderLevel: number;
}

interface RecentMovement {
  movementId: number;
  productId: number;
  sku: string;
  productName: string;
  movementType: string;
  quantity: number;
  quantityBefore: number;
  quantityAfter: number;
  createdAtUtc: string;
}

interface DashboardSummary {
  totalActiveProducts: number;
  totalInventoryValue: number;
  totalOrders: number;
  lowStockCount: number;
  orderStatusSummary: OrderStatusSummary[];
  monthlyOrderSummary: MonthlyOrderSummary[];
  lowStockProducts: LowStockProduct[];
  recentMovements: RecentMovement[];
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reports.html',
  styleUrl: './reports.css',
})
export class Reports implements OnInit {
  private readonly apiUrl = 'https://localhost:7101/api/Reports';

  dashboard: DashboardSummary | null = null;

  loading = false;
  errorMessage = '';
  lastUpdated: Date | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    this.errorMessage = '';

    this.http.get<DashboardSummary>(`${this.apiUrl}/dashboard-summary`).subscribe({
      next: (response) => {
        this.dashboard = response;
        this.lastUpdated = new Date();
        this.loading = false;
      },
      error: (error) => {
        console.error('Reports API Error:', error);
        this.errorMessage = 'Unable to load reports. Please check the API connection.';
        this.loading = false;
      },
    });
  }

  get maxMonthlyOrders(): number {
    if (!this.dashboard?.monthlyOrderSummary?.length) {
      return 1;
    }

    return Math.max(...this.dashboard.monthlyOrderSummary.map((x) => x.totalOrders), 1);
  }

  get maxOrderStatusCount(): number {
    if (!this.dashboard?.orderStatusSummary?.length) {
      return 1;
    }

    return Math.max(...this.dashboard.orderStatusSummary.map((x) => x.count), 1);
  }

  getMonthlyBarWidth(totalOrders: number): number {
    return (totalOrders / this.maxMonthlyOrders) * 100;
  }

  getStatusBarWidth(count: number): number {
    return (count / this.maxOrderStatusCount) * 100;
  }

  getStatusClass(status: string): string {
    return status.toLowerCase().replace(/\s+/g, '-');
  }

  getMovementClass(type: string): string {
    return type.toLowerCase().replace(/\s+/g, '-');
  }

  trackByProduct(index: number, item: LowStockProduct): number {
    return item.productId;
  }

  trackByMovement(index: number, item: RecentMovement): number {
    return item.movementId;
  }

  trackByMonth(index: number, item: MonthlyOrderSummary): string {
    return `${item.year}-${item.month}`;
  }
}
