export interface InventorySummary {
  totalProducts: number;
  totalQuantity: number;
  lowStockCount: number;
  outOfStockCount: number;
  totalInventoryValue: number;
  totalCustomers: number;
  totalOrders: number;
}

export interface LowStockItem {
  inventoryId: number;
  productId: number;
  sku: string;
  productName: string;
  category: string;
  quantity: number;
  reorderLevel: number;
  shortageQuantity: number;
  stockStatus: string;
}

export interface InventoryMovement {
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

export interface DashboardData {
  inventorySummary: InventorySummary;
  lowStock: LowStockItem[];
  movements: InventoryMovement[];
  recentOrders: RecentCustomerOrder[];
}

export interface RecentCustomerOrder {
  id: number;
  orderNumber: string;
  customerId: number;
  customerName: string;
  orderDateUtc: string;
  status: number;
  totalAmount: number;
}
