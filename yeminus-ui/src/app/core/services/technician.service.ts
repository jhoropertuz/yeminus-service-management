import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Technician, CreateTechnicianRequest, UpdateTechnicianRequest } from '../../shared/models/technician.model';
import { ApiResponse } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TechnicianService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/technicians`;

  getAll(): Observable<Technician[]> {
    return this.http.get<ApiResponse<Technician[]>>(this.url).pipe(map(r => r.data ?? []));
  }

  getById(id: string): Observable<Technician> {
    return this.http.get<ApiResponse<Technician>>(`${this.url}/${id}`).pipe(map(r => r.data!));
  }

  create(request: CreateTechnicianRequest): Observable<Technician> {
    return this.http.post<ApiResponse<Technician>>(this.url, request).pipe(map(r => r.data!));
  }

  update(id: string, request: UpdateTechnicianRequest): Observable<Technician> {
    return this.http.put<ApiResponse<Technician>>(`${this.url}/${id}`, request).pipe(map(r => r.data!));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
