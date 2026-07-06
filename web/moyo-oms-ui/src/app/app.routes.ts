import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { PriceStock } from './vendor-products/price-stock';

export const routes: Routes = [
  { path: 'price-stock', component: PriceStock, canActivate: [MsalGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'price-stock' },
];
