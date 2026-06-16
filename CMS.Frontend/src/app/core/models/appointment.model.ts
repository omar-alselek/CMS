export interface Appointment {
  id: number;
  patientId: number;
  doctorId: number;
  receptionistId: number;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  status: string;
  notes: string;
  // للعرض فقط
  patientName?: string;
  doctorName?: string;
}

export interface BookAppointmentRequest {
  patientId: number;
  doctorId: number;
  receptionistId: number;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  notes: string;
}