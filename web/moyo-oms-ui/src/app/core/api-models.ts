export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface VendorProductListItem {
  id: number;
  productReferenceId: number;
  productName: string;
  productCategory: string;
  sellingPrice: number;
  stockQuantity: number;
  availability: string;
  updatedAt: string;
}

export interface OrderListItem {
  id: number;
  clientPortalOrderId: string;
  clientReference: string;
  status: string;
  fulfilmentStatus: string;
  orderTotal: number;
  receivedAt: string;
  allocatedAt: string | null;
}

export interface OrderDetailLine {
  productReferenceId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderStatusHistoryItem {
  previousStatus: string;
  newStatus: string;
  statusNote: string | null;
  changedAt: string;
}

export interface OrderDetail {
  id: number;
  clientPortalOrderId: string;
  clientReference: string;
  status: string;
  fulfilmentStatus: string;
  orderTotal: number;
  receivedAt: string;
  allocatedAt: string | null;
  lines: OrderDetailLine[];
  history: OrderStatusHistoryItem[];
}
