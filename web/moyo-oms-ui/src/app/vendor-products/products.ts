import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';

import { VendorProductListItem } from '../core/api-models';
import { StockAdjustmentDirection, VendorProductService } from './vendor-product.service';

@Component({
  selector: 'app-products',
  imports: [FormsModule, DecimalPipe],
  templateUrl: './products.html',
})
export class Products implements OnInit {
  private readonly service = inject(VendorProductService);

  protected readonly items = signal<VendorProductListItem[]>([]);
  protected readonly total = signal(0);
  protected readonly page = signal(1);
  protected readonly pageSize = 20;
  protected readonly loading = signal(true);

  protected readonly selected = signal<VendorProductListItem | null>(null);
  protected priceInput = 0;
  protected stockInput = 0;
  protected readonly saving = signal(false);
  protected readonly message = signal('');
  protected readonly failed = signal(false);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.service.getMyProducts(this.page(), this.pageSize).subscribe({
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

  protected manage(item: VendorProductListItem): void {
    this.selected.set(item);
    this.priceInput = item.sellingPrice;
    this.stockInput = 0;
    this.message.set('');
  }

  protected close(): void {
    this.selected.set(null);
  }

  protected reprice(): void {
    const item = this.selected();
    if (!item) {
      return;
    }
    this.run(this.service.reprice(item.id, this.priceInput), 'Price updated.');
  }

  protected adjustStock(direction: StockAdjustmentDirection): void {
    const item = this.selected();
    if (!item || this.stockInput <= 0) {
      return;
    }
    this.run(this.service.adjustStock(item.id, direction, this.stockInput), 'Stock updated.');
  }

  protected badge(availability: string): string {
    return 'badge badge--' + availability.toLowerCase();
  }

  private run(request: Observable<void>, successMessage: string): void {
    this.saving.set(true);
    this.message.set('');
    this.failed.set(false);
    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.message.set(successMessage);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.failed.set(true);
        this.message.set(error.error?.detail ?? `Request failed (${error.status}).`);
      },
    });
  }
}
