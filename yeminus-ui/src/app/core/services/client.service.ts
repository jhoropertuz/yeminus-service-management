import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Client, CreateClientRequest, UpdateClientRequest } from '../../shared/models/client.model';
import { ApiResponse } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ClientService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/clients`;

  getAll(): Observable<Client[]> {
    return this.http.get<ApiResponse<Client[]>>(this.url).pipe(map(r => r.data ?? []));
  }

  getById(id: string): Observable<Client> {
    return this.http.get<ApiResponse<Client>>(`${this.url}/${id}`).pipe(map(r => r.data!));
  }

  create(request: CreateClientRequest): Observable<Client> {
    return this.http.post<ApiResponse<Client>>(this.url, request).pipe(map(r => r.data!));
  }

  update(id: string, request: UpdateClientRequest): Observable<Client> {
    return this.http.put<ApiResponse<Client>>(`${this.url}/${id}`, request).pipe(map(r => r.data!));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
