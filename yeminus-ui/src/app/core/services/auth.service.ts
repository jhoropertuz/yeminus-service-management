import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, UserResponse } from '../models/auth.model';
import { ApiResponse } from '../../shared/models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  // platformId must be declared before currentUserSubject (loadUser uses it)
  private readonly platformId = inject(PLATFORM_ID);
  private readonly TOKEN_KEY = 'yeminus_token';
  private readonly USER_KEY = 'yeminus_user';

  private currentUserSubject = new BehaviorSubject<UserResponse | null>(this.loadUser());
  currentUser$ = this.currentUserSubject.asObservable();

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap(response => {
        if (response.success && response.data) {
          this.storage.setItem(this.TOKEN_KEY, response.data.token);
          this.storage.setItem(this.USER_KEY, JSON.stringify(response.data.user));
          this.currentUserSubject.next(response.data.user);
        }
      })
    );
  }

  logout(): void {
    this.storage.removeItem(this.TOKEN_KEY);
    this.storage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.storage.getItem(this.TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getCurrentUser(): UserResponse | null {
    return this.currentUserSubject.value;
  }

  private get storage(): Storage | NoopStorage {
    return isPlatformBrowser(this.platformId) ? localStorage : new NoopStorage();
  }

  private loadUser(): UserResponse | null {
    const stored = this.storage.getItem(this.USER_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}

class NoopStorage {
  getItem(_key: string): null { return null; }
  setItem(_key: string, _value: string): void {}
  removeItem(_key: string): void {}
}
