export enum OrderStatus {
  Pending = 1,
  InProgress = 2,
  Completed = 3
}

export interface Order {
  id: string;
  clientId: string;
  clientName: string;
  clientDocument: string;
  technicianId: string;
  technicianName: string;
  specialty: string;
  status: OrderStatus;
  statusName: string;
  description: string;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderRequest {
  clientId: string;
  technicianId: string;
  description: string;
}

export interface UpdateOrderRequest {
  clientId: string;
  technicianId: string;
  description: string;
  status: OrderStatus;
}

export interface ChangeOrderStatusRequest {
  status: OrderStatus;
}

export interface OrderFilter {
  status?: OrderStatus | null;
  technicianName?: string;
  specialty?: string;
  clientName?: string;
  clientDocument?: string;
  dateFrom?: string;
  dateTo?: string;
}
