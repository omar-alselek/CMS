import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Doctor, RegisterDoctorRequest, DoctorAvailability } from '../models/doctor.model';

@Injectable({ providedIn: 'root' })
export class DoctorService {

  private apiUrl = 'https://localhost:7250/api/doctors';

  constructor(private http: HttpClient) {}

  // جلب كل الأطباء
  getAll() {
    return this.http.get<Doctor[]>(this.apiUrl);
  }

  // إضافة دكتور جديد
  create(request: RegisterDoctorRequest) {
    return this.http.post<Doctor>(this.apiUrl, request);
  }

  // تعديل دكتور
  update(id: number, doctor: Doctor) {
    return this.http.put<Doctor>(`${this.apiUrl}/${id}`, doctor);
  }

  // حذف دكتور
  delete(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

     // جلب أوقات الدكتور
getAvailability(doctorId: number) {
  return this.http.get<DoctorAvailability[]>(`${this.apiUrl}/${doctorId}/availability`);
}

// حفظ أوقات الدكتور
setAvailability(doctorId: number, availabilities: DoctorAvailability[]) {
  return this.http.post(`${this.apiUrl}/${doctorId}/availability`, availabilities);
}
}