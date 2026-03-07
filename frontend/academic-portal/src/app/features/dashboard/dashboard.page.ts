import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  standalone: true,
  selector: 'app-dashboard-page',
  imports: [CommonModule, RouterLink],
  template: `
    <section class="hero-shell relative overflow-hidden rounded-3xl border border-slate-200/80 bg-white p-6 shadow-[0_16px_45px_rgba(10,35,64,0.1)] sm:p-8">
      <div class="pointer-events-none absolute -right-14 -top-16 h-44 w-44 rounded-full bg-[color:var(--umg-gold-500)]/18 blur-3xl"></div>
      <div class="pointer-events-none absolute -left-20 bottom-0 h-48 w-48 rounded-full bg-[color:var(--umg-navy-700)]/15 blur-3xl"></div>

      <div class="relative">
        <p class="text-xs font-semibold uppercase tracking-[0.15em] text-[color:var(--umg-navy-700)]">Portal UMG Guatemala</p>
        <h1 class="font-display mt-2 text-3xl font-extrabold tracking-tight text-[color:var(--umg-navy-900)] sm:text-4xl">
          Bienvenido, {{ auth.me()?.fullName || auth.me()?.email }}
        </h1>
        <p class="mt-2 max-w-3xl text-sm text-muted">
          Panel académico institucional con autenticación segura, control de roles y trazabilidad de gestiones estudiantiles.
        </p>

        <div class="mt-5 flex flex-wrap gap-2">
          <span class="info-badge">Carnet: {{ auth.me()?.carnet || 'Pendiente' }}</span>
          <span class="info-badge">Rol: {{ auth.me()?.role || 'N/A' }}</span>
          <span class="info-badge">{{ auth.me()?.programName || 'Programa no configurado' }}</span>
          <span class="info-badge">{{ auth.me()?.campusName || 'Sede no configurada' }}</span>
          <span class="info-badge">Jornada: {{ currentShiftLabel() }}</span>
        </div>
      </div>
    </section>

    <section class="mt-6 grid gap-4 md:grid-cols-3">
      <article class="stat-card">
        <div class="stat-icon bg-emerald-100 text-emerald-700">
          <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M16.704 5.29a1 1 0 010 1.42l-7.41 7.41a1 1 0 01-1.42 0l-3.16-3.16a1 1 0 111.42-1.42l2.45 2.45 6.7-6.7a1 1 0 011.42 0z" clip-rule="evenodd" />
          </svg>
        </div>
        <p class="mt-4 text-xs font-semibold uppercase tracking-[0.08em] text-slate-500">Estado Académico</p>
        <h2 class="font-display mt-2 text-2xl font-bold text-emerald-700">Activo</h2>
        <p class="mt-1 text-sm text-muted">Servicios estudiantiles habilitados para tu perfil.</p>
      </article>

      <article class="stat-card">
        <div class="stat-icon bg-sky-100 text-sky-700">
          <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path d="M10 2a8 8 0 100 16 8 8 0 000-16zm1 4a1 1 0 10-2 0v.07a3 3 0 00-1.243 5.48l2.883 1.442a1 1 0 11-.894 1.789l-1.3-.65a1 1 0 10-.894 1.789l1.448.724V17a1 1 0 102 0v-.07a3 3 0 001.243-5.48l-2.883-1.442a1 1 0 11.894-1.789l.6.3a1 1 0 10.894-1.789L11 6.176V6z" />
          </svg>
        </div>
        <p class="mt-4 text-xs font-semibold uppercase tracking-[0.08em] text-slate-500">Moneda Oficial</p>
        <h2 class="font-display mt-2 text-2xl font-bold text-[color:var(--umg-navy-900)]">GTQ</h2>
        <p class="mt-1 text-sm text-muted">Órdenes, pagos y reportes en quetzales.</p>
      </article>

      <article class="stat-card">
        <div class="stat-icon bg-amber-100 text-amber-700">
          <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path d="M10 2a1 1 0 01.707.293l6 6A1 1 0 0117 9v8a1 1 0 01-1 1h-3v-4a3 3 0 10-6 0v4H4a1 1 0 01-1-1V9a1 1 0 01.293-.707l6-6A1 1 0 0110 2z" />
          </svg>
        </div>
        <p class="mt-4 text-xs font-semibold uppercase tracking-[0.08em] text-slate-500">Institución</p>
        <h2 class="font-display mt-2 text-xl font-bold text-[color:var(--umg-navy-900)]">UMG Guatemala</h2>
        <p class="mt-1 text-sm text-muted">Portal institucional para gestiones académicas.</p>
      </article>
    </section>

    <section class="mt-8" *ngIf="auth.me()?.role === 'Student'">
      <header class="mb-4 flex items-center justify-between">
        <h3 class="section-title text-xl">Accesos Rápidos</h3>
        <p class="text-xs font-semibold uppercase tracking-[0.1em] text-slate-500">Gestiones estudiantiles</p>
      </header>

      <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <a routerLink="/transfers" class="action-card group">
          <div class="action-icon bg-indigo-100 text-indigo-700">
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M7 3a1 1 0 011 1v1h4V4a1 1 0 112 0v1h1a2 2 0 012 2v2h-2V7H5v8h4v2H5a2 2 0 01-2-2V7a2 2 0 012-2h1V4a1 1 0 011-1z" />
              <path d="M13 10a1 1 0 00-1 1v1H9a1 1 0 100 2h3v1a1 1 0 102 0v-1h1a1 1 0 100-2h-1v-1a1 1 0 00-1-1z" />
            </svg>
          </div>
          <h4 class="mt-4 font-display text-lg font-bold text-[color:var(--umg-navy-900)]">Gestionar traslados</h4>
          <p class="mt-2 text-sm text-muted">Solicita cambio de sede y jornada con validación de cupos.</p>
        </a>

        <a routerLink="/courses" class="action-card group">
          <div class="action-icon bg-cyan-100 text-cyan-700">
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M4 3a2 2 0 00-2 2v9a3 3 0 003 3h11a1 1 0 100-2H5a1 1 0 010-2h11a2 2 0 002-2V5a2 2 0 00-2-2H4z" />
            </svg>
          </div>
          <h4 class="mt-4 font-display text-lg font-bold text-[color:var(--umg-navy-900)]">Asignación de cursos</h4>
          <p class="mt-2 text-sm text-muted">Gestiona cursos por ciclo, atrasados y jornadas por curso.</p>
        </a>

        <a routerLink="/payments" class="action-card group">
          <div class="action-icon bg-emerald-100 text-emerald-700">
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M3 5a2 2 0 012-2h10a2 2 0 012 2v2H3V5z" />
              <path fill-rule="evenodd" d="M3 9h14v6a2 2 0 01-2 2H5a2 2 0 01-2-2V9zm3 2a1 1 0 000 2h2a1 1 0 100-2H6z" clip-rule="evenodd" />
            </svg>
          </div>
          <h4 class="mt-4 font-display text-lg font-bold text-[color:var(--umg-navy-900)]">Mis pagos</h4>
          <p class="mt-2 text-sm text-muted">Consulta órdenes, estados y simulación de pago con tarjeta.</p>
        </a>

        <a routerLink="/certificates" class="action-card group">
          <div class="action-icon bg-amber-100 text-amber-700">
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M4 3a2 2 0 00-2 2v7a2 2 0 002 2h3l2 3 2-3h5a2 2 0 002-2V5a2 2 0 00-2-2H4zm3 4a1 1 0 000 2h6a1 1 0 100-2H7z" clip-rule="evenodd" />
            </svg>
          </div>
          <h4 class="mt-4 font-display text-lg font-bold text-[color:var(--umg-navy-900)]">Certificación digital</h4>
          <p class="mt-2 text-sm text-muted">Solicita, genera y descarga certificados institucionales.</p>
        </a>
      </div>
    </section>

    <section class="mt-8" *ngIf="auth.me()?.role === 'Admin'">
      <a routerLink="/admin-reports" class="btn-primary inline-flex items-center gap-2 px-5 py-3 text-sm">
        Abrir panel administrativo
      </a>
    </section>
  `
})
export class DashboardPage {
  readonly auth = inject(AuthService);

  constructor() {
    if (this.auth.isAuthenticated() && !this.auth.me()) {
      this.auth.loadMe().subscribe();
    }
  }

  currentShiftLabel(): string {
    const shift = this.auth.me()?.shiftName;
    if (shift === 'Saturday') {
      return 'Sábado';
    }

    if (shift === 'Sunday') {
      return 'Domingo';
    }

    return shift || 'Jornada no configurada';
  }
}
