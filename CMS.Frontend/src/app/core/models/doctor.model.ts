export interface Doctor {
  id: number;
  fullName: string;
  email: string;
  phone: string;
  specialization: string;
  licenseNumber: string;
}

export interface RegisterDoctorRequest {
  doctor: Doctor;
  temporaryPassword: string;
}

export interface DoctorAvailability {
  id: number;
  doctorId: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
}