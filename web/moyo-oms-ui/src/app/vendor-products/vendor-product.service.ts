import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { apiBaseUrl } from '../core/api.config';
import { PagedResult, VendorProductListItem } from '../core/api-models';

export type StockAdjustmentDirection = 'Increase' | 'Decrease';

@Injectable({ providedIn: 'root' })
export class VendorProductService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${apiBaseUrl()}/api/vendor-products`;

  getMyProducts(page: number, pageSize: number): Observable<PagedResult<VendorProductListItem>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<VendorProductListItem>>(this.baseUrl, { params });
  }

  reprice(vendorProductId: number, newPrice: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${vendorProductId}/price`, { newPrice });
  }

  adjustStock(
    vendorProductId: number,
    direction: StockAdjustmentDirection,
    quantity: number,
  ): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${vendorProductId}/stock`, { direction, quantity });
  }
}
