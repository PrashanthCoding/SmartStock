import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { Login } from './features/auth/login/login';
import { CustomerOrders } from './features/customer-orders/customer-orders';
import { Customers } from './features/customers/customers';
import { Dashboard } from './features/dashboard/dashboard';
import { Inventories } from './features/inventories/inventories';
import { Products } from './features/products/products';
import { Reports } from './features/reports/reports';
import { StockMovements } from './features/stock-movements/stock-movements';
import { MainLayout } from './layouts/main-layout/main-layout';

export const routes: Routes = [
  {
    path: 'login',
    component: Login,
  },

  {
    path: '',
    component: MainLayout,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        component: Dashboard,
      },
      {
        path: 'products',
        component: Products,
      },
      {
        path: 'inventory',
        component: Inventories,
      },
      {
        path: 'stock-movements',
        component: StockMovements,
      },
      {
        path: 'customers',
        component: Customers,
      },
      {
        path: 'customer-orders',
        component: CustomerOrders,
      },
      {
        path: 'reports',
        component: Reports,
      },
      {
        path: 'live-market',
        loadComponent: () => import('./features/live-market/live-market').then((m) => m.LiveMarket),
      },
    ],
  },

  {
    path: '**',
    redirectTo: '',
  },
];
