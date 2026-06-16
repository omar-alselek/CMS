import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Patient } from '../models/patient.model';

@Injectable({ providedIn: 'root' })
export class PatientService {

  private apiUrl = 'https://localhost:7250/api/patients';

  constructor(private http: HttpClient) {}

  // جلب كل المرضى
  getAll() {
    return this.http.get<Patient[]>(this.apiUrl);
  }

  // البحث عن مريض
  search(query: string) {
    return this.http.get<Patient[]>(`${this.apiUrl}/search?q=${query}`);
  }

  // إضافة مريض
  create(patient: Patient) {
    return this.http.post<Patient>(this.apiUrl, patient);
  }

  // تعديل مريض
  update(id: number, patient: Patient) {
    return this.http.put<Patient>(`${this.apiUrl}/${id}`, patient);
  }

  // عرض التاريخ المرضي
  getHistory(id: number) {
    return this.http.get<any>(`${this.apiUrl}/${id}/history`);
  }
}