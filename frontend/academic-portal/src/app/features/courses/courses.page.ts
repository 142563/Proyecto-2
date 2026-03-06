import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { API_BASE_URL } from '../../core/config/api.config';
import {
  ApiEnvelope,
  CourseDto,
  EnrollmentSummaryResponse,
  OverdueCourseDto
} from '../../shared/models/api.models';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'app-courses-page',
  imports: [CommonModule, StatusBadgeComponent],
  template: `
    <section class="grid gap-6 lg:grid-cols-2">
      <article class="panel p-6">
        <h2 class="section-title text-lg">Cursos del Pensum</h2>
        <p class="mt-1 text-sm text-muted">Pensum oficial por ciclo. Selecciona cursos extra o atrasados para generar orden de pago.</p>

        <div class="mt-4 space-y-4">
          <section *ngFor="let cycle of cycleOrder">
            <div *ngIf="coursesByCycle.get(cycle)?.length">
              <h3 class="font-display text-sm font-bold uppercase tracking-wide text-[color:var(--umg-navy-700)]">Ciclo {{ cycle }}</h3>
              <div class="mt-2 space-y-2">
                <label
                  *ngFor="let course of coursesByCycle.get(cycle)"
                  class="block rounded-lg border border-slate-200 p-3"
                >
                  <div class="flex items-center justify-between gap-3">
                    <span class="font-semibold text-slate-800">{{ course.code }} - {{ course.name }}</span>
                    <input type="checkbox" [checked]="selectedCourseIds.has(course.id)" (change)="toggleCourse(course.id)" />
                  </div>
                  <p class="mt-1 text-xs text-muted">
                    {{ course.credits }} créditos · {{ course.hoursPerWeek }} h/semana · {{ course.hoursTotal }} h totales
                    <span *ngIf="course.isLab">· Con laboratorio</span>
                  </p>
                  <p class="mt-1 text-xs text-muted">Prerrequisitos: {{ course.prerequisiteSummary }}</p>
                  <span *ngIf="course.isOverdue" class="mt-2 inline-block rounded bg-amber-100 px-2 py-0.5 text-xs text-amber-700">Atrasado</span>
                </label>
              </div>
            </div>
          </section>
        </div>

        <button class="btn-primary mt-4 w-full px-4 py-2" (click)="createEnrollment()">
          Generar asignación + orden de pago
        </button>
      </article>

      <article class="panel p-6">
        <h2 class="section-title text-lg">Cursos Atrasados Detectados</h2>
        <ul class="mt-4 space-y-2">
          <li *ngFor="let item of overdue" class="rounded-lg border border-slate-200 p-3">{{ item.code }} - {{ item.name }}</li>
        </ul>

        <p *ngIf="message" class="mt-4 text-sm text-emerald-700">{{ message }}</p>
        <p *ngIf="error" class="mt-2 text-sm text-rose-700">{{ error }}</p>
      </article>
    </section>

    <section class="panel mt-6 p-6">
      <h3 class="section-title text-lg">Mis solicitudes de asignación</h3>
      <div class="mt-4 overflow-x-auto">
        <table class="table-clean w-full text-left text-sm">
          <thead>
            <tr class="border-b border-slate-200">
              <th class="py-2">Fecha</th>
              <th>Tipo</th>
              <th>Total</th>
              <th>Vence</th>
              <th>Estado</th>
              <th class="text-right">Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of enrollments" class="border-b border-slate-100">
              <td class="py-2">{{ item.createdAt | date:'short' }}</td>
              <td>{{ item.enrollmentType }}</td>
              <td>Q{{ item.totalAmount | number:'1.2-2' }} {{ item.currency }}</td>
              <td>{{ item.paymentExpiresAt | date:'short' }}</td>
              <td><app-status-badge [label]="item.status"></app-status-badge></td>
              <td class="text-right">
                <button
                  *ngIf="item.status === 'PendingPayment'"
                  class="btn-danger px-3 py-1 text-xs"
                  (click)="cancelEnrollment(item.enrollmentId)"
                >
                  Cancelar
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class CoursesPage {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  courses: CourseDto[] = [];
  coursesByCycle = new Map<number, CourseDto[]>();
  cycleOrder: number[] = [];
  overdue: OverdueCourseDto[] = [];
  enrollments: EnrollmentSummaryResponse[] = [];
  selectedCourseIds = new Set<number>();
  message = '';
  error = '';

  constructor() {
    this.loadPensum();
    this.loadOverdue();
    this.loadEnrollments();
  }

  loadPensum(): void {
    this.http.get<ApiEnvelope<CourseDto[]>>(`${this.baseUrl}/courses/pensum`).subscribe((response) => {
      this.courses = response.success ? response.data : [];
      this.groupByCycle();
    });
  }

  loadOverdue(): void {
    this.http.get<ApiEnvelope<OverdueCourseDto[]>>(`${this.baseUrl}/courses/overdue`).subscribe((response) => {
      this.overdue = response.success ? response.data : [];
    });
  }

  loadEnrollments(): void {
    this.http.get<ApiEnvelope<EnrollmentSummaryResponse[]>>(`${this.baseUrl}/enrollments/my`).subscribe((response) => {
      this.enrollments = response.success ? response.data : [];
    });
  }

  toggleCourse(courseId: number): void {
    if (this.selectedCourseIds.has(courseId)) {
      this.selectedCourseIds.delete(courseId);
      return;
    }

    this.selectedCourseIds.add(courseId);
  }

  private groupByCycle(): void {
    const map = new Map<number, CourseDto[]>();
    for (const course of this.courses) {
      const cycle = course.cycle ?? 0;
      const group = map.get(cycle) ?? [];
      group.push(course);
      map.set(cycle, group);
    }

    this.coursesByCycle = map;
    this.cycleOrder = Array.from(map.keys()).sort((a, b) => a - b);
  }

  createEnrollment(): void {
    this.message = '';
    this.error = '';

    this.http
      .post<ApiEnvelope<{ paymentOrderId: string; totalAmount: number; currency: string }>>(`${this.baseUrl}/enrollments`, {
        courseIds: Array.from(this.selectedCourseIds)
      })
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo crear la asignación.';
            return;
          }

          this.message = `Asignación creada. Orden ${response.data.paymentOrderId} - Total Q${response.data.totalAmount.toFixed(2)} ${response.data.currency}`;
          this.selectedCourseIds.clear();
          this.loadEnrollments();
        },
        error: (error: HttpErrorResponse) => {
          this.error = error.error?.error?.message ?? 'Error de conexión.';
        }
      });
  }

  cancelEnrollment(enrollmentId: string): void {
    this.message = '';
    this.error = '';

    this.http.post<ApiEnvelope<{ enrollmentId: string }>>(`${this.baseUrl}/enrollments/${enrollmentId}/cancel`, {}).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No se pudo cancelar la asignación.';
          return;
        }

        this.message = 'Asignación cancelada correctamente.';
        this.loadEnrollments();
      },
      error: (error: HttpErrorResponse) => {
        this.error = error.error?.error?.message ?? 'Error de conexión.';
      }
    });
  }
}
