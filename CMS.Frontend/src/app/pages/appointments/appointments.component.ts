import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppointmentService } from '../../core/services/appointment.service';
import { PatientService } from '../../core/services/patient.service';
import { DoctorService } from '../../core/services/doctor.service';
import { PrescriptionService } from '../../core/services/prescription.service';
import { SidebarComponent } from '../../shared/components/sidebar.component';
import { Appointment } from '../../core/models/appointment.model';
import { Patient } from '../../core/models/patient.model';
import { Doctor } from '../../core/models/doctor.model';
import { Prescription } from '../../core/models/prescription.model';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent],
  templateUrl: './appointments.component.html'
})
export class AppointmentsComponent implements OnInit {

  appointments: Appointment[] = [];
  patients: Patient[] = [];
  doctors: Doctor[] = [];

  role = localStorage.getItem('role');
  personId = Number(localStorage.getItem('personId'));

  showBookForm = false;
  showRescheduleForm = false;
  showPrescriptionForm = false;
  showViewPrescription = false;

  selectedAppointment: Appointment | null = null;
  selectedPrescription: Prescription | null = null;

  errorMessage = '';
  successMessage = '';
  // Patient search for booking form
  patientSearchQuery = '';
  patientSearchResults: Patient[] = [];
  selectedPatientName = '';
  
  searchPatients() {
    if (!this.patientSearchQuery.trim()) return;
    this.patientService.search(this.patientSearchQuery).subscribe({
      next: (data) => {
        this.patientSearchResults = data;
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }
  
  selectPatient(patient: Patient) {
    this.bookForm.patientId = patient.id;
    this.selectedPatientName = patient.fullName + ' — ' + patient.phone;
    this.patientSearchResults = []; // أخفي النتائج بعد الاختيار
    this.patientSearchQuery = '';
    this.cdr.detectChanges();
  }
  today = new Date().toISOString().substring(0, 10);

  bookForm = {
    patientId: 0,
    doctorId: 0,
    appointmentDate: '',
    startTime: '',
    endTime: '',
    notes: ''
  };

  rescheduleForm = {
    appointmentDate: '',
    startTime: '',
    endTime: ''
  };

  prescriptionForm = {
    medication: '',
    notes: ''
  };

  constructor(
    private appointmentService: AppointmentService,
    private patientService: PatientService,
    private doctorService: DoctorService,
    private prescriptionService: PrescriptionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
  this.loadAppointments();
  this.loadPatients(); // ←  الكل يحتاجها
  if (this.role === 'Receptionist') {
    this.loadDoctors();
  }
}

  loadAppointments() {
    // الدكتور يرى مواعيده فقط
    if (this.role === 'Doctor') {
      this.appointmentService.getByDoctor(this.personId).subscribe({
        next: (data) => {
          this.appointments = data;
          this.cdr.detectChanges();
        },
        error: () => this.errorMessage = 'Failed to load appointments.'
      });
    } else {
      // Receptionist يرى كل المواعيد
      this.appointmentService.getAll().subscribe({
        next: (data) => {
          this.appointments = data;
          this.cdr.detectChanges();
        },
        error: () => this.errorMessage = 'Failed to load appointments.'
      });
    }
  }

  loadPatients() {
    this.patientService.getAll().subscribe({
      next: (data) => {
        this.patients = data;
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  loadDoctors() {
    this.doctorService.getAll().subscribe({
      next: (data) => {
        this.doctors = data;
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  openBookForm() {
    this.bookForm = { patientId: 0, doctorId: 0, appointmentDate: '', startTime: '', endTime: '', notes: '' };
    this.showBookForm = true;
    this.patientSearchQuery = '';
    this.patientSearchResults = [];
    this.selectedPatientName = '';
    this.showRescheduleForm = false;
    this.showPrescriptionForm = false;
    this.errorMessage = '';
    this.cdr.detectChanges();
  }

  book() {
    if (!this.bookForm.patientId || !this.bookForm.doctorId ||
        !this.bookForm.appointmentDate || !this.bookForm.startTime ||
        !this.bookForm.endTime) {
      this.errorMessage = 'Please fill all required fields.';
      return;
    }

    const request = {
      patientId: this.bookForm.patientId,
      doctorId: this.bookForm.doctorId,
      receptionistId: this.personId,
      appointmentDate: this.bookForm.appointmentDate + 'T12:00:00',
      startTime: this.bookForm.startTime + ':00',
      endTime: this.bookForm.endTime + ':00',
      notes: this.bookForm.notes
    };

    this.appointmentService.book(request).subscribe({
      next: () => {
        this.successMessage = 'Appointment booked successfully!';
        this.showBookForm = false;
        this.loadAppointments();
      },
      error: (err) => this.errorMessage = err.error?.message || 'Booking failed.'
    });
  }

  confirm(id: number) {
    this.appointmentService.confirm(id).subscribe({
      next: () => {
        this.successMessage = 'Appointment confirmed!';
        this.loadAppointments();
      },
      error: (err) => this.errorMessage = err.error?.message || 'Failed to confirm.'
    });
  }

  cancel(id: number) {
    this.appointmentService.cancel(id).subscribe({
      next: () => {
        this.successMessage = 'Appointment cancelled!';
        this.loadAppointments();
      },
      error: (err) => this.errorMessage = err.error?.message || 'Failed to cancel.'
    });
  }

  openReschedule(appointment: Appointment) {
    this.selectedAppointment = appointment;
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.rescheduleForm = {
      appointmentDate: tomorrow.toISOString().substring(0, 10),
      startTime: appointment.startTime.substring(0, 5),
      endTime: appointment.endTime.substring(0, 5)
    };
    this.showRescheduleForm = true;
    this.showBookForm = false;
    this.showPrescriptionForm = false;
    this.cdr.detectChanges();
  }

  reschedule() {
    if (!this.selectedAppointment) return;
    const dateStr = this.rescheduleForm.appointmentDate + 'T12:00:00';
    this.appointmentService.reschedule(
      this.selectedAppointment.id,
      dateStr,
      this.rescheduleForm.startTime,
      this.rescheduleForm.endTime
    ).subscribe({
      next: () => {
        this.successMessage = 'Appointment rescheduled!';
        this.showRescheduleForm = false;
        this.loadAppointments();
      },
      error: (err) => this.errorMessage = err.error?.message || 'Failed to reschedule.'
    });
  }

  // فتح فورم الوصفة — للدكتور فقط
  openPrescriptionForm(appointment: Appointment) {
    this.selectedAppointment = appointment;
    this.prescriptionForm = { medication: '', notes: '' };
    this.showPrescriptionForm = true;
    this.showBookForm = false;
    this.showRescheduleForm = false;
    this.showViewPrescription = false;
    this.errorMessage = '';
    this.cdr.detectChanges();
  }

  savePrescription() {
    if (!this.selectedAppointment) return;

    if (!this.prescriptionForm.medication.trim()) {
      this.errorMessage = 'Medication is required.';
      return;
    }

    const request = {
      appointmentId: this.selectedAppointment.id,
      patientId: this.selectedAppointment.patientId,
      doctorId: this.personId,
      medication: this.prescriptionForm.medication,
      notes: this.prescriptionForm.notes
    };

    this.prescriptionService.create(request).subscribe({
      next: () => {
        this.successMessage = 'Prescription created successfully!';
        this.showPrescriptionForm = false;
        this.loadAppointments();
      },
      error: (err) => this.errorMessage = err.error?.message || 'Failed to create prescription.'
    });
  }

  // عرض وصفة موجودة
  viewPrescription(appointmentId: number) {
  this.prescriptionService.getByAppointment(appointmentId).subscribe({
    next: (data) => {
      this.selectedPrescription = data;
      this.showViewPrescription = true;
      this.showPrescriptionForm = false;
      this.cdr.detectChanges();
    },
    error: () => {
      // لا توجد وصفة — هذا طبيعي
      this.selectedPrescription = null;
      this.showViewPrescription = false;
      this.errorMessage = 'No prescription for this appointment yet.';
    }
  });
}

  getStatusColor(status: string): string {
    switch (status) {
      case 'Scheduled': return '#fef3e8';
      case 'Confirmed': return '#e8f4fd';
      case 'Completed': return '#e8f8f0';
      case 'Cancelled': return '#fde8e8';
      case 'Rescheduled': return '#f0e8fd';
      default: return '#f8f9fa';
    }
  }

  getStatusTextColor(status: string): string {
    switch (status) {
      case 'Scheduled': return '#854f0b';
      case 'Confirmed': return '#0066cc';
      case 'Completed': return '#0f6e56';
      case 'Cancelled': return '#cc0000';
      case 'Rescheduled': return '#6600cc';
      default: return '#666';
    }
  }

  getPatientName(id: number): string {
    return this.patients.find(p => p.id === id)?.fullName || '—';
  }

  getDoctorName(id: number): string {
    return this.doctors.find(d => d.id === id)?.fullName || '—';
  }
}