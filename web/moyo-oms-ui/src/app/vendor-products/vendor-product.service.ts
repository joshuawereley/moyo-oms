import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface RepriceVendorProduct {
  newPrice: number;
}

@Injectable({ providedIn: 'root' })
export class VendorProductService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5222/api/vendor-products';

  reprice(vendorProductId: number, request: RepriceVendorProduct): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${vendorProductId}/price`, request);
  }
}
