import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { ApiEnvelope, AuthResponse, MeResponse } from '../../shared/models/api.models';
import { API_BASE_URL } from '../config/api.config';

const TOKEN_KEY = 'academic_token';
const USER_KEY = 'academic_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = API_BASE_URL;

  readonly me = signal<MeResponse | null>(this.readUser());

  login(email: string, password: string): Observable<ApiEnvelope<AuthResponse>> {
    return this.http.post<ApiEnvelope<AuthResponse>>(`${this.apiBaseUrl}/auth/login`, { email, password }).pipe(
      tap((response) => {
        if (!response.success) {
          return;
        }

        localStorage.setItem(TOKEN_KEY, response.data.accessToken);
        this.loadMe().subscribe();
      })
    );
  }

  loadMe(): Observable<ApiEnvelope<MeResponse>> {
    return this.http.get<ApiEnvelope<MeResponse>>(`${this.apiBaseUrl}/me`).pipe(
      tap((response) => {
        if (response.success) {
          this.me.set(response.data);
          localStorage.setItem(USER_KEY, JSON.stringify(response.data));
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.me.set(null);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem(TOKEN_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  hasRole(roles: string[]): boolean {
    const current = this.me();
    if (!current) {
      return false;
    }

    return roles.includes(current.role);
  }

  private readUser(): MeResponse | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as MeResponse;
    } catch {
      return null;
    }
  }
}
