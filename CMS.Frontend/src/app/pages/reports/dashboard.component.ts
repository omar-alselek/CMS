import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { SidebarComponent } from '../../shared/components/sidebar.component';
import { BaseChartDirective } from 'ng2-charts'; // ← الاستيراد الصحيح للإصدارات الحديثة

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, BaseChartDirective, SidebarComponent], // ← أضف BaseChartDirective هنا
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {

  fullName = localStorage.getItem('fullName');
  
  // متغيرات الإحصائيات
  totalPatients = 0;
  totalDoctors = 0;
  totalAppointments = 0;
  scheduledAppointments = 0;
  completedAppointments = 0;
  cancelledAppointments = 0;

  // ← إعدادات الشارت →
  public appointmentsChartLabels = ['Scheduled', 'Completed', 'Cancelled'];
  public appointmentsChartData = {
    labels: this.appointmentsChartLabels,
    datasets: [{
      data: [0, 0, 0], 
      backgroundColor: ['#ffc107', '#198754', '#dc3545'],
      hoverBackgroundColor: ['#ffca2c', '#1a9e63', '#e35051'],
      borderWidth: 0,
    }]
  };

    public appointmentsChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '70%',
    plugins: {
      legend: {
        position: 'bottom' as const, // ← أضفنا as const هنا
        labels: {
          padding: 20,
          usePointStyle: true,
          pointStyle: 'circle' as const, // ← أضفنا as const هنا أيضاً
          font: { size: 13 }
        }
      }
    }
  };

  private apiUrl = 'https://localhost:7250/api';

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadDashboard();
  }

    loadDashboard() {
    this.http.get<any>(`${this.apiUrl}/reports/dashboard`)
      .subscribe({
        next: (data) => {
          this.totalPatients = data.totalPatients;
          this.totalDoctors = data.totalDoctors;
          this.totalAppointments = data.totalAppointments;
          this.scheduledAppointments = data.scheduledCount;
          this.completedAppointments = data.completedCount;
          this.cancelledAppointments = data.cancelledCount;

          // ← الحل: إنشاء كائن بيانات جديد بالكامل لتحديث الشارت →
          this.appointmentsChartData = {
            labels: this.appointmentsChartLabels,
            datasets: [{
              data: [
                this.scheduledAppointments,
                this.completedAppointments,
                this.cancelledAppointments
              ],
              backgroundColor: ['#ffc107', '#198754', '#dc3545'],
              hoverBackgroundColor: ['#ffca2c', '#1a9e63', '#e35051'],
              borderWidth: 0,
            }]
          };

          this.cdr.detectChanges();
        },
        error: (err) => {
          console.log('Error:', err);
        }
      });
  }

  logout() {
    this.authService.logout();
  }

  goTo(page: string) {
    this.router.navigate([`/${page}`]);
  }
}