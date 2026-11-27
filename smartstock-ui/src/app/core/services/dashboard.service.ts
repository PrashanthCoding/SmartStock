import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';

import {
  DashboardData,
  InventoryMovement,
  InventorySummary,
  LowStockItem,
  RecentCustomerOrder,
} from '../models/dashboard';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);

  private readonly apiUrl = 'https://localhost:7101/api';

  getDashboardData(): Observable<DashboardData> {
    return forkJoin({
      inventorySummary: this.http.get<InventorySummary>(`${this.apiUrl}/Inventories/summary`),

      lowStock: this.http.get<LowStockItem[]>(`${this.apiUrl}/Inventories/low-stock`),

      movements: this.http.get<InventoryMovement[]>(`${this.apiUrl}/InventoryMovements`),

      recentOrders: this.http.get<RecentCustomerOrder[]>(`${this.apiUrl}/CustomerOrders`),
    });
  }
}
