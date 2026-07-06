import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VendorProductService } from './vendor-product.service';

@Component({
  selector: 'app-price-stock',
  imports: [FormsModule],
  templateUrl: './price-stock.html',
})
export class PriceStock {
  private readonly vendorProducts = inject(VendorProductService);

  protected vendorProductId = 1;
  protected newPrice = 0;
  protected readonly status = signal('');

  protected reprice(): void {
    this.status.set('Saving…');

    this.vendorProducts.reprice(this.vendorProductId, { newPrice: this.newPrice }).subscribe({
      next: () => this.status.set('Price updated successfully.'),
      error: (error: HttpErrorResponse) =>
        this.status.set(`Failed: ${error.status} ${error.statusText}`.trim()),
    });
  }
}
