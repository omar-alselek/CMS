import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {

  role = localStorage.getItem('role');
  fullName = localStorage.getItem('fullName');

  constructor(private router: Router, private authService: AuthService) {}

  goTo(page: string) {
    this.router.navigate([`/${page}`]);
  }

  isActive(page: string): boolean {
    return this.router.url.includes(page);
  }

  logout() {
    this.authService.logout();
  }
}