import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';

import { Dashboard } from './dashboard/dashboard';
import { OrderDetailComponent } from './orders/order-detail';
import { Orders } from './orders/orders';
import { Products } from './vendor-products/products';

export const routes: Routes = [
  { path: 'dashboard', component: Dashboard, canActivate: [MsalGuard] },
  { path: 'products', component: Products, canActivate: [MsalGuard] },
  { path: 'orders', component: Orders, canActivate: [MsalGuard] },
  { path: 'orders/:id', component: OrderDetailComponent, canActivate: [MsalGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '**', redirectTo: 'dashboard' },
];
