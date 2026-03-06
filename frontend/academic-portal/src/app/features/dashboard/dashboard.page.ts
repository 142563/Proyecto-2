import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  standalone: true,
  selector: 'app-dashboard-page',
  imports: [CommonModule, RouterLink],
  template: `
    <section class="panel p-6">
      <p class="text-xs font-semibold uppercase tracking-[0.12em] text-[color:var(--umg-navy-700)]">Portal UMG Guatemala</p>
      <h1 class="section-title mt-2 text-2xl">Bienvenido, {{ auth.me()?.email }}</h1>
      <p class="text-muted mt-1">Rol actual: {{ auth.me()?.role }} · Sesión segura JWT + roles + cierre por inactividad.</p>
    </section>

    <section class="mt-6 grid gap-4 md:grid-cols-3">
      <article class="panel p-5">
        <p class="text-xs font-semibold uppercase text-slate-500">Estado</p>
        <h2 class="mt-2 font-display text-xl font-bold text-emerald-700">Activo</h2>
        <p class="mt-1 text-sm text-muted">Servicios académicos disponibles.</p>
      </article>

      <article class="panel p-5">
        <p class="text-xs font-semibold uppercase text-slate-500">Moneda</p>
        <h2 class="mt-2 font-display text-xl font-bold text-[color:var(--umg-navy-900)]">Quetzales (GTQ)</h2>
        <p class="mt-1 text-sm text-muted">Órdenes y catálogos en Q.</p>
      </article>

      <article class="panel p-5">
        <p class="text-xs font-semibold uppercase text-slate-500">Institución</p>
        <h2 class="mt-2 font-display text-xl font-bold text-[color:var(--umg-navy-900)]">Universidad Mariano Gálvez</h2>
        <p class="mt-1 text-sm text-muted">Campus y centros universitarios configurados.</p>
      </article>
    </section>

    <section class="mt-8 grid gap-4 md:grid-cols-2 lg:grid-cols-4" *ngIf="auth.me()?.role === 'Student'">
      <a routerLink="/transfers" class="panel p-4 transition hover:-translate-y-0.5">Gestionar traslados</a>
      <a routerLink="/courses" class="panel p-4 transition hover:-translate-y-0.5">Asignación de cursos</a>
      <a routerLink="/payments" class="panel p-4 transition hover:-translate-y-0.5">Mis pagos</a>
      <a routerLink="/certificates" class="panel p-4 transition hover:-translate-y-0.5">Certificación digital</a>
    </section>

    <section class="mt-8" *ngIf="auth.me()?.role === 'Admin'">
      <a routerLink="/admin-reports" class="btn-primary inline-block px-5 py-3 text-sm">Abrir panel administrativo</a>
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
}
