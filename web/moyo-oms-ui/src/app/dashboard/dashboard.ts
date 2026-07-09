import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { OrderListItem } from '../core/api-models';
import { OrderService } from '../orders/order.service';
import { VendorProductService } from '../vendor-products/vendor-product.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, DecimalPipe, DatePipe],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  private readonly products = inject(VendorProductService);
  private readonly orders = inject(OrderService);

  protected readonly productCount = signal<number | null>(null);
  protected readonly orderCount = signal<number | null>(null);
  protected readonly recentOrders = signal<OrderListItem[]>([]);
  protected readonly loading = signal(true);

  ngOnInit(): void {
    this.products.getMyProducts(1, 1).subscribe({
      next: (result) => this.productCount.set(result.totalCount),
      error: () => this.productCount.set(0),
    });

    this.orders.getMyOrders(1, 6).subscribe({
      next: (result) => {
        this.orderCount.set(result.totalCount);
        this.recentOrders.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected badge(status: string): string {
    return 'badge badge--' + status.toLowerCase();
  }
}
