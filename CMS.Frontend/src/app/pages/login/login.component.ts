import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  username = '';
  password = '';
  errorMessage = '';
  isLoading = false;

  constructor(private authService: AuthService, private router: Router) {}

  onLogin() {
    this.errorMessage = '';
    this.isLoading = true;

    this.authService.login({ username: this.username, password: this.password })
      .subscribe({
        next: (response) => {
            this.authService.saveToken(response);
          
            // كل role يروح لصفحة مختلفة
            if (response.role === 'ClinicManager') {
              this.router.navigate(['/dashboard']);
            } else if (response.role === 'Doctor') {
              this.router.navigate(['/appointments']);
            } else if (response.role === 'Receptionist') {
              this.router.navigate(['/patients']);
            }
          },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'اسم المستخدم أو كلمة المرور غلط';
        }
      });
  }
}