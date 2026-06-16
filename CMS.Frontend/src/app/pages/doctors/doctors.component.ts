import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DoctorService } from '../../core/services/doctor.service';
import { Doctor, RegisterDoctorRequest, DoctorAvailability } from '../../core/models/doctor.model';
import { SidebarComponent } from '../../shared/components/sidebar.component';

@Component({
  selector: 'app-doctors',
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent], // ← أضف هنا
  templateUrl: './doctors.component.html'
})
export class DoctorsComponent implements OnInit {

  doctors: Doctor[] = [];
  filteredDoctors: Doctor[] = [];
  searchQuery = '';
  showForm = false;
  isEditing = false;
  errorMessage = '';
  successMessage = '';

  form: Doctor = {
    id: 0,
    fullName: '',
    email: '',
    phone: '',
    specialization: '',
    licenseNumber: ''
  };

  temporaryPassword = 'Doctor@123';

  constructor(
    private doctorService: DoctorService,
    private router: Router,
    private cdr: ChangeDetectorRef  // ← أضف هذا
  ) {}

  ngOnInit() {
    this.loadDoctors();
  }

  loadDoctors() {
    this.doctorService.getAll().subscribe({
      next: (data) => {
        this.doctors = data;
        this.filteredDoctors = data; // ← أضف هذا السطر
        this.cdr.detectChanges();  // ← أضف هذا
      },
      error: () => this.errorMessage = 'Failed to load doctors.'
    });
  }

    search() {
    const q = this.searchQuery.toLowerCase().trim();
    if (!q) {
      this.filteredDoctors = this.doctors;
    } else {
      this.filteredDoctors = this.doctors.filter(d =>
        d.fullName.toLowerCase().includes(q) ||
        d.specialization.toLowerCase().includes(q)
      );
    }
    this.cdr.detectChanges();
  }

  clearSearch() {
    this.searchQuery = '';
    this.filteredDoctors = this.doctors;
    this.cdr.detectChanges();
  }

  openAddForm() {
    this.isEditing = false;
    this.form = { id: 0, fullName: '', email: '', phone: '', specialization: '', licenseNumber: '' };
    this.showForm = true;
    this.errorMessage = '';
  }

  openEditForm(doctor: Doctor) {
    this.isEditing = true;
    this.form = { ...doctor };
    this.showForm = true;
    this.errorMessage = '';
  }

  save() {
    if (this.isEditing) {
      this.doctorService.update(this.form.id, this.form).subscribe({
        next: () => {
          this.successMessage = 'Doctor updated successfully!';
          this.showForm = false;
          this.loadDoctors();
        },
        error: (err) => this.errorMessage = err.error?.message || 'Update failed.'
      });
    } else {
      this.doctorService.create({ doctor: this.form, temporaryPassword: this.temporaryPassword }).subscribe({
        next: () => {
          this.successMessage = 'Doctor registered successfully!';
          this.showForm = false;
          this.loadDoctors();
        },
        error: (err) => this.errorMessage = err.error?.message || 'Registration failed.'
      });
    }
  }

  delete(id: number) {
    if (confirm('Are you sure you want to delete this doctor?')) {
      this.doctorService.delete(id).subscribe({
        next: () => {
          this.successMessage = 'Doctor deleted successfully!';
          this.loadDoctors();
        },
        error: (err) => this.errorMessage = err.error?.message || 'Delete failed.'
      });
    }
  }
  // متغيرات الأوقات
showAvailability = false;
selectedDoctor: Doctor | null = null;
availabilities: DoctorAvailability[] = [];

days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

// فتح صفحة الأوقات
openAvailability(doctor: Doctor) {
  this.selectedDoctor = doctor;
  this.showAvailability = true;
  this.showForm = false;

  this.availabilities = [
    { id: 0, doctorId: doctor.id, dayOfWeek: 0, startTime: '09:00:00', endTime: '17:00:00' },
    { id: 0, doctorId: doctor.id, dayOfWeek: 1, startTime: '09:00:00', endTime: '17:00:00' },
    { id: 0, doctorId: doctor.id, dayOfWeek: 2, startTime: '09:00:00', endTime: '17:00:00' },
    { id: 0, doctorId: doctor.id, dayOfWeek: 3, startTime: '09:00:00', endTime: '17:00:00' },
    { id: 0, doctorId: doctor.id, dayOfWeek: 4, startTime: '09:00:00', endTime: '17:00:00' },
  ];
}

// حفظ الأوقات
saveAvailability() {
  if (!this.selectedDoctor) return;

  // نضيف :00 للوقت عشان يصير TimeSpan صح
  const formatted = this.availabilities.map(a => ({
    ...a,
    startTime: a.startTime.length === 5 ? a.startTime + ':00' : a.startTime,
    endTime: a.endTime.length === 5 ? a.endTime + ':00' : a.endTime,
  }));

  this.doctorService.setAvailability(this.selectedDoctor.id, formatted)
    .subscribe({
      next: () => {
        this.successMessage = 'Schedule saved successfully!';
        this.showAvailability = false;
      },
      error: (err) => this.errorMessage = err.error?.message || 'Failed to save schedule.'
    });
}
  goBack() {
    this.router.navigate(['/dashboard']);
  }
}