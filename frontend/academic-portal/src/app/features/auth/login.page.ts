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
    <section class="relative mx-auto mt-6 max-w-5xl overflow-hidden rounded-[1.75rem] border border-slate-200/80 bg-white/65 shadow-[0_18px_50px_rgba(10,35,64,0.12)] backdrop-blur-sm">
      <div class="pointer-events-none absolute -left-16 top-8 h-48 w-48 rounded-full bg-[color:var(--umg-gold-500)]/15 blur-2xl"></div>
      <div class="pointer-events-none absolute -right-16 -top-16 h-52 w-52 rounded-full bg-[color:var(--umg-navy-700)]/18 blur-2xl"></div>

      <div class="relative grid lg:min-h-[560px] lg:grid-cols-[1.08fr_0.92fr]">
        <aside class="hidden h-full border-r border-white/45 bg-gradient-to-br from-[color:var(--umg-navy-900)] via-[#12355f] to-[color:var(--umg-navy-700)] p-10 text-white lg:flex lg:flex-col lg:justify-between">
          <div>
            <div class="inline-flex items-center gap-3 rounded-full border border-white/25 bg-white/10 px-4 py-2">
              <img src="assets/umg-shield.png" alt="Escudo UMG" class="h-8 w-8 rounded-full border border-white/40 bg-white/90 p-0.5" />
              <span class="text-xs font-semibold uppercase tracking-[0.12em] text-slate-100">Portal MIUMG Estudiantil</span>
            </div>

            <h1 class="font-display mt-8 text-4xl font-extrabold leading-tight">Ingreso Institucional</h1>
            <p class="mt-3 max-w-md text-sm leading-relaxed text-slate-200">
              Accede a tus servicios académicos en un entorno seguro con autenticación JWT, control de roles y cierre por inactividad.
            </p>
          </div>

          <div class="space-y-3 text-sm text-slate-100">
            <div class="rounded-xl border border-white/20 bg-white/10 px-4 py-3">Credenciales institucionales verificadas</div>
            <div class="rounded-xl border border-white/20 bg-white/10 px-4 py-3">Protección y trazabilidad de solicitudes</div>
            <div class="rounded-xl border border-white/20 bg-white/10 px-4 py-3">Universidad Mariano Gálvez de Guatemala</div>
          </div>
        </aside>

        <div class="flex h-full items-center justify-center p-5 sm:p-8 lg:p-10">
          <div class="w-full max-w-md rounded-2xl border border-slate-200/80 bg-white/95 p-7 shadow-[0_16px_40px_rgba(13,36,64,0.1)] sm:p-8">
            <p class="text-xs font-semibold uppercase tracking-[0.13em] text-[color:var(--umg-navy-700)]">Universidad Mariano Gálvez de Guatemala</p>
            <h2 class="font-display mt-2 text-3xl font-extrabold text-[color:var(--umg-navy-900)]">Inicio de sesión</h2>
            <p class="mt-2 text-sm text-muted">Ingresa con tu correo institucional y contraseña.</p>

            <form class="mt-6 space-y-4" [formGroup]="form" (ngSubmit)="submit()">
              <div>
                <label class="mb-1.5 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Correo institucional</label>
                <input
                  class="input-control"
                  [class.border-rose-300]="form.controls.email.touched && form.controls.email.invalid"
                  formControlName="email"
                  type="email"
                  autocomplete="username"
                  placeholder="usuario@umg.edu.gt"
                />
              </div>

              <div>
                <label class="mb-1.5 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Contraseña</label>
                <div class="relative">
                  <input
                    class="input-control pr-20"
                    [class.border-rose-300]="form.controls.password.touched && form.controls.password.invalid"
                    formControlName="password"
                    [type]="showPassword ? 'text' : 'password'"
                    autocomplete="current-password"
                    placeholder="********"
                  />
                  <button
                    type="button"
                    class="absolute inset-y-0 right-3 text-xs font-semibold text-[color:var(--umg-navy-700)] hover:text-[color:var(--umg-navy-900)]"
                    (click)="togglePasswordVisibility()"
                  >
                    {{ showPassword ? 'Ocultar' : 'Mostrar' }}
                  </button>
                </div>
              </div>

              <p *ngIf="error" class="rounded-xl border border-rose-200 bg-rose-50 px-3 py-2.5 text-sm text-rose-700">{{ error }}</p>

              <button class="btn-primary mt-1 inline-flex w-full items-center justify-center gap-2 px-4 py-3 text-sm" [disabled]="loading">
                <span *ngIf="loading" class="h-4 w-4 animate-spin rounded-full border-2 border-white/40 border-t-white"></span>
                {{ loading ? 'Validando acceso...' : 'Iniciar sesión' }}
              </button>
            </form>
          </div>
        </div>
      </div>
    </section>
  `
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  error = '';
  showPassword = false;

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

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
