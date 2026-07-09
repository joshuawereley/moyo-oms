import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { OrderDetail } from '../core/api-models';
import { OrderService } from './order.service';

@Component({
  selector: 'app-order-detail',
  imports: [FormsModule, RouterLink, DecimalPipe, DatePipe],
  templateUrl: './order-detail.html',
})
export class OrderDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(OrderService);

  protected readonly order = signal<OrderDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);

  protected readonly statuses = ['InProgress', 'Completed', 'Cancelled'];
  protected targetStatus = 'InProgress';
  protected statusNote = '';
  protected readonly saving = signal(false);
  protected readonly message = signal('');
  protected readonly failed = signal(false);

  private orderId = 0;

  ngOnInit(): void {
    this.orderId = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.service.getOrder(this.orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        if (error.status === 404) {
          this.notFound.set(true);
        }
      },
    });
  }

  protected changeStatus(): void {
    this.saving.set(true);
    this.message.set('');
    this.failed.set(false);
    this.service.changeStatus(this.orderId, this.targetStatus, this.statusNote || undefined).subscribe({
      next: () => {
        this.saving.set(false);
        this.message.set('Status updated.');
        this.statusNote = '';
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.failed.set(true);
        this.message.set(error.error?.detail ?? `Request failed (${error.status}).`);
      },
    });
  }

  protected badge(status: string): string {
    return 'badge badge--' + status.toLowerCase();
  }
}
