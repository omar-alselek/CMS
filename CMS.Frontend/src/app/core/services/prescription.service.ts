import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Prescription, CreatePrescriptionRequest } from '../models/prescription.model';

@Injectable({ providedIn: 'root' })
export class PrescriptionService {

  private apiUrl = 'https://localhost:7250/api/prescriptions';

  constructor(private http: HttpClient) {}

  // إنشاء وصفة
  create(request: CreatePrescriptionRequest) {
    return this.http.post<Prescription>(this.apiUrl, request);
  }

  // جلب وصفة موعد معين
  getByAppointment(appointmentId: number) {
    return this.http.get<Prescription>(`${this.apiUrl}/appointment/${appointmentId}`);
  }
}