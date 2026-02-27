import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  standalone: true,
  selector: 'app-dashboard-page',
  imports: [CommonModule, RouterLink],
  template: `
    <section class="grid gap-6 md:grid-cols-3">
      <article class="rounded-2xl bg-white p-6 shadow-sm">
        <p class="text-xs font-semibold uppercase text-slate-500">Usuario</p>
        <h2 class="mt-2 text-xl font-bold text-slate-900">{{ auth.me()?.email }}</h2>
        <p class="mt-1 text-sm text-slate-500">Rol: {{ auth.me()?.role }}</p>
      </article>

      <article class="rounded-2xl bg-white p-6 shadow-sm">
        <p class="text-xs font-semibold uppercase text-slate-500">Estado</p>
        <h2 class="mt-2 text-xl font-bold text-emerald-700">Activo</h2>
        <p class="mt-1 text-sm text-slate-500">Sesion protegida por JWT + roles.</p>
      </article>

      <article class="rounded-2xl bg-white p-6 shadow-sm">
        <p class="text-xs font-semibold uppercase text-slate-500">Accesos</p>
        <h2 class="mt-2 text-xl font-bold text-slate-900">Modulos Academicos</h2>
        <p class="mt-1 text-sm text-slate-500">Traslados, cursos, pagos, certificados y reportes.</p>
      </article>
    </section>

    <section class="mt-8 grid gap-4 md:grid-cols-2 lg:grid-cols-3" *ngIf="auth.me()?.role === 'Student'">
      <a routerLink="/transfers" class="rounded-xl bg-white p-4 shadow-sm hover:bg-slate-50">Gestionar traslados</a>
      <a routerLink="/courses" class="rounded-xl bg-white p-4 shadow-sm hover:bg-slate-50">Asignar cursos</a>
      <a routerLink="/payments" class="rounded-xl bg-white p-4 shadow-sm hover:bg-slate-50">Ver pagos</a>
      <a routerLink="/certificates" class="rounded-xl bg-white p-4 shadow-sm hover:bg-slate-50">Certificacion digital</a>
    </section>

    <section class="mt-8" *ngIf="auth.me()?.role === 'Admin'">
      <a routerLink="/admin-reports" class="inline-block rounded-xl bg-slate-900 px-5 py-3 text-white">Abrir reportes administrativos</a>
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
