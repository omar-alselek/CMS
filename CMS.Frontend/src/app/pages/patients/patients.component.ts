import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PatientService } from '../../core/services/patient.service';
import { Patient } from '../../core/models/patient.model';
import { SidebarComponent } from '../../shared/components/sidebar.component';

@Component({
  selector: 'app-patients',
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent], // ← أضف هنا
  templateUrl: './patients.component.html'
  
})
export class PatientsComponent implements OnInit {
  role = localStorage.getItem('role'); 
  patients: Patient[] = [];
  showForm = false;
  isEditing = false;
  showHistory = false;
  selectedHistory: any = null;
  errorMessage = '';
  successMessage = '';
  searchQuery = '';

  form: Patient = {
    id: 0,
    fullName: '',
    phone: '',
    email: '',
    address: '',
    bloodType: '',
    dateOfBirth: ''
  };

  bloodTypes = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];

  constructor(
    private patientService: PatientService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
  this.loadAll();
  
}

loadAll() {
  this.patientService.getAll().subscribe({
    next: (data) => {
      this.patients = data;
      this.cdr.detectChanges();
    },
    error: () => this.errorMessage = 'Failed to load patients.'
  });
}

search() {
  if (this.searchQuery.trim() === '') {
    this.loadAll();
    return;
  }
  this.patientService.search(this.searchQuery).subscribe({
    next: (data) => {
      this.patients = data;
      this.cdr.detectChanges();
    },
    error: () => this.errorMessage = 'Search failed.'
  });
}

  openAddForm() {
  this.isEditing = false;
  this.showHistory = false;
  this.form = { id: 0, fullName: '', phone: '', email: '', address: '', bloodType: '', dateOfBirth: '' };
  this.showForm = true;
  this.errorMessage = '';
  this.cdr.detectChanges(); // ← أضف هذا
}

openEditForm(patient: Patient) {
  this.isEditing = true;
  this.showHistory = false;
  this.form = { ...patient };
  if (this.form.dateOfBirth) {
    this.form.dateOfBirth = this.form.dateOfBirth.substring(0, 10);
  }
  this.showForm = true;
  this.errorMessage = '';
  this.cdr.detectChanges(); // ← أضف هذا
}

  save() {
  // تأكد من صيغة التاريخ
  const patientData = {
    ...this.form,
    dateOfBirth: this.form.dateOfBirth ? new Date(this.form.dateOfBirth).toISOString() : null
  };

  if (this.isEditing) {
    this.patientService.update(this.form.id, patientData).subscribe({
      next: () => {
        this.successMessage = 'Patient updated successfully!';
        this.showForm = false;
        this.loadAll();
      },
      error: (err) => this.errorMessage = err.error?.message || 'Update failed.'
    });
  } else {
    this.patientService.create(patientData).subscribe({
      next: () => {
        this.successMessage = 'Patient registered successfully!';
        this.showForm = false;
        this.loadAll();
      },
      error: (err) => this.errorMessage = err.error?.message || 'Registration failed.'
    });
  }
}

  viewHistory(id: number) {
    this.showForm = false;
    this.patientService.getHistory(id).subscribe({
      next: (data) => {
        this.selectedHistory = data;
        this.showHistory = true;
        this.cdr.detectChanges();
      },
      error: () => this.errorMessage = 'Failed to load history.'
    });
  }

  closeHistory() {
    this.showHistory = false;
    this.selectedHistory = null;
  }

  goTo(page: string) {
    this.router.navigate([`/${page}`]);
  }
}