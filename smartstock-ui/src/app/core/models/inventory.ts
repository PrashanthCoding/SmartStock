export interface Inventory {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
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

export interface InventorySummary {
  totalProducts: number;
  totalQuantity: number;
  lowStockCount: number;
  outOfStockCount: number;
  totalInventoryValue: number;
  totlaCustomers: number;
  totalOrders: number;
}
