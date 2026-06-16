export interface Prescription {
  id: number;
  appointmentId: number;
  patientId: number;
  doctorId: number;
  medication: string;
  notes: string;
  prescriptionDate: string;
}

export interface CreatePrescriptionRequest {
  appointmentId: number;
  patientId: number;
  doctorId: number;
  medication: string;
  notes: string;
}