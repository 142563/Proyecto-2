import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { IdleTimeoutService } from './core/services/idle-timeout.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  // Instantiate idle timeout service once at app root.
  private readonly idleTimeout = inject(IdleTimeoutService);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }

  isStudent(): boolean {
    return this.auth.me()?.role === 'Student';
  }

  isAdmin(): boolean {
    return this.auth.me()?.role === 'Admin';
  }
}
