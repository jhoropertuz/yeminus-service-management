import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  Order,
  CreateOrderRequest,
  UpdateOrderRequest,
  ChangeOrderStatusRequest,
  OrderFilter
} from '../../shared/models/order.model';
import { ApiResponse } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/orders`;

  getAll(filter?: OrderFilter): Observable<Order[]> {
    let params = new HttpParams();
    if (filter) {
      if (filter.status != null) params = params.set('status', filter.status.toString());
      if (filter.technicianName) params = params.set('technicianName', filter.technicianName);
      if (filter.specialty) params = params.set('specialty', filter.specialty);
      if (filter.clientName) params = params.set('clientName', filter.clientName);
      if (filter.clientDocument) params = params.set('clientDocument', filter.clientDocument);
      if (filter.dateFrom) params = params.set('dateFrom', filter.dateFrom);
      if (filter.dateTo) params = params.set('dateTo', filter.dateTo);
    }
    return this.http.get<ApiResponse<Order[]>>(this.url, { params }).pipe(map(r => r.data ?? []));
  }

  getById(id: string): Observable<Order> {
    return this.http.get<ApiResponse<Order>>(`${this.url}/${id}`).pipe(map(r => r.data!));
  }

  create(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<ApiResponse<Order>>(this.url, request).pipe(map(r => r.data!));
  }

  update(id: string, request: UpdateOrderRequest): Observable<Order> {
    return this.http.put<ApiResponse<Order>>(`${this.url}/${id}`, request).pipe(map(r => r.data!));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  changeStatus(id: string, request: ChangeOrderStatusRequest): Observable<Order> {
    return this.http.patch<ApiResponse<Order>>(`${this.url}/${id}/status`, request).pipe(map(r => r.data!));
  }
}
