export interface Patient {
  id: number;
  fullName: string;
  phone: string;
  email: string;
  address: string;
  bloodType: string;
  dateOfBirth: string | null;
}