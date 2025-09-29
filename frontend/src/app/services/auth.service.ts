import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { LoginRequest, AuthResponse } from '../models/conversation.model';
import { environment } from '../../environments/environment';
import { Router } from '@angular/router';

//! only for SHOW PURPOSES
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private _baseUrl = `${environment.apiUrl}/auth`;
  private _authHeader$;
  private _isAuthenticated$ = new BehaviorSubject<boolean>(false);
  private _http: HttpClient = inject(HttpClient);
  private _router = inject(Router);

  constructor() {
    this._authHeader$ = new BehaviorSubject<string | null>(this.getHeaderFromStorage());

    this._authHeader$.subscribe((value) => {
      this._isAuthenticated$.next(!!value);
      if (value) {
        window.localStorage.setItem('authHeader', value);
      }
    });
  }

  get isAuthenticated$(): Observable<boolean> {
    return this._isAuthenticated$.asObservable();
  }

  get isAuthenticated(): boolean {
    return this._isAuthenticated$.value;
  }

  get authHeader(): string | null {
    return this._authHeader$.value;
  }

  getHeaderFromStorage(): string | null {
    return window.localStorage.getItem('authHeader');
  }

  login(loginRequest: LoginRequest): Observable<AuthResponse> {
    return this._http.post<AuthResponse>(`${this._baseUrl}/login`, loginRequest).pipe(
      tap((r) => {
        this._authHeader$.next(r.authHeader);
      })
    );
  }

  logout() {
    this._authHeader$.next(null);
    window.localStorage.removeItem('authHeader');
    this._router.navigate(['/login']);
  }

  setAuthenticated(isAuthenticated: boolean): void {
    this._isAuthenticated$.next(isAuthenticated);
  }
}
