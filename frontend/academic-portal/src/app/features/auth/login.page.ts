import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  standalone: true,
  selector: 'app-login-page',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="mx-auto mt-16 max-w-md rounded-2xl bg-white p-8 shadow-lg">
      <h1 class="text-2xl font-bold text-slate-900">Ingreso Institucional</h1>
      <p class="mt-1 text-sm text-slate-500">Usa tu correo institucional y contrasena.</p>

      <form class="mt-6 space-y-4" [formGroup]="form" (ngSubmit)="submit()">
        <div>
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Correo</label>
          <input
            class="w-full rounded-lg border border-slate-300 px-3 py-2"
            formControlName="email"
            type="email"
            placeholder="usuario@universidad.edu"
          />
        </div>

        <div>
          <label class="mb-1 block text-xs font-semibold uppercase text-slate-500">Contrasena</label>
          <input
            class="w-full rounded-lg border border-slate-300 px-3 py-2"
            formControlName="password"
            type="password"
            placeholder="********"
          />
        </div>

        <p *ngIf="error" class="rounded-lg bg-rose-100 px-3 py-2 text-sm text-rose-700">{{ error }}</p>

        <button
          class="w-full rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white"
          [disabled]="loading"
        >
          {{ loading ? 'Ingresando...' : 'Iniciar sesion' }}
        </button>
      </form>

      <p class="mt-5 text-xs text-slate-500">
        Demo: admin@universidad.edu / Admin123! | ana.gomez@alumnos.universidad.edu / Student123!
      </p>
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
          this.error = response.error?.message ?? 'No fue posible iniciar sesion.';
          return;
        }

        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.loading = false;
        this.error = 'Error de conexion con el servidor.';
      }
    });
  }
}
