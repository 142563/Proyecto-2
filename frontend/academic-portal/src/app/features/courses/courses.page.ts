import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({
  standalone: true,
  selector: 'app-courses-page',
  imports: [CommonModule],
  template: `
    <section class="grid gap-6 lg:grid-cols-2">
      <article class="rounded-2xl bg-white p-6 shadow-sm">
        <h2 class="text-lg font-bold">Cursos del Pensum</h2>
        <div class="mt-4 space-y-2">
          <label *ngFor="let course of courses" class="flex items-center justify-between rounded-lg border p-3">
            <span>{{ course.code }} - {{ course.name }}</span>
            <input type="checkbox" [checked]="selectedCourseIds.has(course.id)" (change)="toggleCourse(course.id)" />
          </label>
        </div>
        <button class="mt-4 rounded-lg bg-slate-900 px-4 py-2 text-white" (click)="createEnrollment()">
          Generar asignacion + orden de pago
        </button>
      </article>

      <article class="rounded-2xl bg-white p-6 shadow-sm">
        <h2 class="text-lg font-bold">Cursos Atrasados</h2>
        <ul class="mt-4 space-y-2">
          <li *ngFor="let item of overdue" class="rounded-lg border p-3">{{ item.code }} - {{ item.name }}</li>
        </ul>

        <p *ngIf="message" class="mt-4 text-sm text-emerald-700">{{ message }}</p>
        <p *ngIf="error" class="mt-2 text-sm text-rose-700">{{ error }}</p>
      </article>
    </section>
  `
})
export class CoursesPage {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  courses: any[] = [];
  overdue: any[] = [];
  selectedCourseIds = new Set<number>();
  message = '';
  error = '';

  constructor() {
    this.loadPensum();
    this.loadOverdue();
  }

  loadPensum(): void {
    this.http.get<any>(`${this.baseUrl}/courses/pensum`).subscribe((response) => {
      this.courses = response.success ? response.data : [];
    });
  }

  loadOverdue(): void {
    this.http.get<any>(`${this.baseUrl}/courses/overdue`).subscribe((response) => {
      this.overdue = response.success ? response.data : [];
    });
  }

  toggleCourse(courseId: number): void {
    if (this.selectedCourseIds.has(courseId)) {
      this.selectedCourseIds.delete(courseId);
      return;
    }

    this.selectedCourseIds.add(courseId);
  }

  createEnrollment(): void {
    this.message = '';
    this.error = '';

    this.http
      .post<any>(`${this.baseUrl}/enrollments`, {
        courseIds: Array.from(this.selectedCourseIds)
      })
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo crear la asignacion.';
            return;
          }

          this.message = `Asignacion creada. Orden: ${response.data.paymentOrderId} - Total: $${response.data.totalAmount}`;
        },
        error: () => {
          this.error = 'Error de conexion.';
        }
      });
  }
}
