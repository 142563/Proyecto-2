import { Injectable, NgZone, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Injectable({ providedIn: 'root' })
export class IdleTimeoutService implements OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly ngZone = inject(NgZone);

  private timeoutRef: ReturnType<typeof setTimeout> | null = null;
  private readonly timeoutMs = 15 * 60 * 1000;
  private readonly events = ['mousemove', 'keydown', 'click', 'scroll', 'touchstart'];
  private readonly eventHandler = () => this.resetTimer();

  constructor() {
    this.events.forEach((eventName) =>
      window.addEventListener(eventName, this.eventHandler, { passive: true })
    );
    this.resetTimer();
  }

  ngOnDestroy(): void {
    this.events.forEach((eventName) => window.removeEventListener(eventName, this.eventHandler));
    if (this.timeoutRef) {
      clearTimeout(this.timeoutRef);
    }
  }

  private resetTimer(): void {
    if (this.timeoutRef) {
      clearTimeout(this.timeoutRef);
    }

    this.timeoutRef = setTimeout(() => {
      this.ngZone.run(() => {
        if (!this.authService.isAuthenticated()) {
          return;
        }

        this.authService.logout();
        this.router.navigate(['/auth/login']);
      });
    }, this.timeoutMs);
  }
}
