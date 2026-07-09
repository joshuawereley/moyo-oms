import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { OrderListItem } from '../core/api-models';
import { OrderService } from './order.service';

@Component({
  selector: 'app-orders',
  imports: [RouterLink, DecimalPipe, DatePipe],
  templateUrl: './orders.html',
})
export class Orders implements OnInit {
  private readonly service = inject(OrderService);

  protected readonly items = signal<OrderListItem[]>([]);
  protected readonly total = signal(0);
  protected readonly page = signal(1);
  protected readonly pageSize = 20;
  protected readonly loading = signal(true);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.service.getMyOrders(this.page(), this.pageSize).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.total.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected get pageCount(): number {
    return Math.max(1, Math.ceil(this.total() / this.pageSize));
  }

  protected prev(): void {
    if (this.page() > 1) {
      this.page.update((value) => value - 1);
      this.load();
    }
  }

  protected next(): void {
    if (this.page() < this.pageCount) {
      this.page.update((value) => value + 1);
      this.load();
    }
  }

  protected badge(status: string): string {
    return 'badge badge--' + status.toLowerCase();
  }
}
