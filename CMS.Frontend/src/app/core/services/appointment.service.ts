import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Appointment, BookAppointmentRequest } from '../models/appointment.model';

@Injectable({ providedIn: 'root' })
export class AppointmentService {

  private apiUrl = 'https://localhost:7250/api/appointments';

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<Appointment[]>(this.apiUrl);
  }

  getByDoctor(doctorId: number) {
    return this.http.get<Appointment[]>(`${this.apiUrl}/doctor/${doctorId}`);
  }

  book(request: any) {
    return this.http.post<Appointment>(this.apiUrl, request);
  }

  confirm(id: number) {
    return this.http.put(`${this.apiUrl}/${id}/confirm`, {});
  }

  cancel(id: number) {
    return this.http.put(`${this.apiUrl}/${id}/cancel`, {});
  }

  reschedule(id: number, date: string, startTime: string, endTime: string) {
    return this.http.put(`${this.apiUrl}/${id}/reschedule`, {
      newDate: date,
      newStart: startTime + ':00',
      newEnd: endTime + ':00'
    });
  }
}