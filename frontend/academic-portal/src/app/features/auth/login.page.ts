import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  standalone: true,
  selector: 'app-login-page',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="mx-auto mt-12 max-w-md rounded-2xl border border-slate-200 bg-white p-8 shadow-xl">
      <p class="text-xs font-semibold uppercase tracking-[0.12em] text-[color:var(--umg-navy-700)]">Universidad Mariano Gálvez de Guatemala</p>
      <h1 class="font-display mt-2 text-3xl font-extrabold text-[color:var(--umg-navy-900)]">Ingreso Institucional</h1>
      <p class="mt-2 text-sm text-muted">Usa tu correo institucional y contraseña.</p>

      <form class="mt-6 space-y-4" [formGroup]="form" (ngSubmit)="submit()">
        <div>
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Correo</label>
          <input class="input-control" formControlName="email" type="email" placeholder="usuario@umg.edu.gt" />
        </div>

        <div>
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Contraseña</label>
          <input class="input-control" formControlName="password" type="password" placeholder="********" />
        </div>

        <p *ngIf="error" class="rounded-lg bg-rose-100 px-3 py-2 text-sm text-rose-700">{{ error }}</p>

        <button class="btn-primary w-full px-4 py-2.5 text-sm" [disabled]="loading">
          {{ loading ? 'Ingresando...' : 'Iniciar sesión' }}
        </button>
      </form>
    </div>
  `
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  error = '';

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }

    this.error = '';
    this.loading = true;

    const { email, password } = this.form.getRawValue();
    this.auth.login(email ?? '', password ?? '').subscribe({
      next: (response) => {
        this.loading = false;
        if (!response.success) {
          this.error = response.error?.message ?? 'No fue posible iniciar sesión.';
          return;
        }

        this.router.navigate(['/dashboard']);
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.error =
          error.error?.error?.message ??
          error.error?.message ??
          (error.status === 0 ? 'Error de conexión con el servidor.' : 'No fue posible iniciar sesión.');
      }
    });
  }
}
