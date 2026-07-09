import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../core/api.config';
import { OrderDetail, OrderListItem, PagedResult } from '../core/api-models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/orders`;

  getMyOrders(page: number, pageSize: number): Observable<PagedResult<OrderListItem>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<OrderListItem>>(this.baseUrl, { params });
  }

  getOrder(orderId: number): Observable<OrderDetail> {
    return this.http.get<OrderDetail>(`${this.baseUrl}/${orderId}`);
  }

  changeStatus(orderId: number, targetStatus: string, statusNote?: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${orderId}/status`, {
      targetStatus,
      statusNote: statusNote ?? null,
    });
  }
}
