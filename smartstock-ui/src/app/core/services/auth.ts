import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_ENDPOINTS } from './api';

export interface LoginRequest {
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);

  login(credentials: LoginRequest): Observable<any> {
    return this.http.post<any>(API_ENDPOINTS.login, credentials).pipe(
      tap((response) => {
        const token =
          response?.token ??
          response?.accessToken ??
          response?.jwtToken ??
          response?.data?.token ??
          response?.data?.accessToken;

        if (!token) {
          throw new Error(
            'JWT token not found in login response. Check your API response structure.',
          );
        }

        localStorage.setItem('smartstock_token', token);
      }),
    );
  }

  getToken(): string | null {
    return localStorage.getItem('smartstock_token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    localStorage.removeItem('smartstock_token');
  }
}
