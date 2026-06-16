import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { LoginRequest, LoginResponse } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private apiUrl = 'https://localhost:7250/api';

  constructor(private http: HttpClient, private router: Router) {}

  // إرسال بيانات الدخول
  login(request: LoginRequest) {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, request);
  }

  // حفظ الـ Token بعد الدخول
  saveToken(response: LoginResponse) {
  localStorage.setItem('token', response.token);
  localStorage.setItem('role', response.role);
  localStorage.setItem('fullName', response.fullName);
  localStorage.setItem('userId', response.userId.toString());
  localStorage.setItem('personId', response.personId?.toString() || '');
}

  // هل المستخدم داخل؟
  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  // جلب الـ Token
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  // تسجيل الخروج
  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}