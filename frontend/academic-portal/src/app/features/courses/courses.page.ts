import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { API_BASE_URL } from '../../core/config/api.config';
import {
  ApiEnvelope,
  CourseDto,
  CreateEnrollmentRequest,
  EnrollmentSummaryResponse,
  MeResponse,
  OverdueCourseDto,
  ShiftName
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
        <p class="mt-1 text-sm text-muted">
          Selecciona un ciclo para ver cursos disponibles. Puedes elegir jornada por curso para tu asignación.
        </p>

        <div class="mt-4">
          <label class="text-xs font-semibold uppercase tracking-wide text-[color:var(--umg-navy-700)]">Selecciona ciclo</label>
          <div class="mt-2 flex flex-wrap gap-2">
            <button
              *ngFor="let cycle of cycleOrder; trackBy: trackByCycle"
              type="button"
              class="rounded-full border px-3 py-1 text-sm transition"
              [ngClass]="selectedCycle === cycle ? 'border-slate-900 bg-slate-900 text-white' : 'border-slate-300 text-slate-700'"
              (click)="setCycle(cycle)"
            >
              Ciclo {{ cycle }}
            </button>
          </div>
        </div>

        <div class="mt-4 space-y-2">
          <label *ngFor="let course of visibleCourses; trackBy: trackByCourseId" class="block rounded-lg border border-slate-200 p-3">
            <div class="flex items-start justify-between gap-3">
              <div>
                <span class="font-semibold text-slate-800">{{ course.code }} - {{ course.name }}</span>
                <p class="mt-1 text-xs text-muted">
                  {{ course.credits }} créditos · {{ course.hoursPerWeek }} h/semana · {{ course.hoursTotal }} h totales
                  <span *ngIf="course.isLab">· Con laboratorio</span>
                </p>
                <p class="mt-1 text-xs text-muted">Prerrequisitos: {{ course.prerequisiteSummary }}</p>
                <span *ngIf="course.isOverdue" class="mt-2 inline-block rounded bg-amber-100 px-2 py-0.5 text-xs text-amber-700">Atrasado</span>
                <span *ngIf="course.isApproved" class="mt-2 ml-2 inline-block rounded bg-emerald-100 px-2 py-0.5 text-xs text-emerald-700">Aprobado</span>
              </div>
              <input
                type="checkbox"
                [checked]="isSelected(course.id)"
                [disabled]="!isSelectableCourse(course)"
                (change)="toggleCourse(course.id)"
              />
            </div>

            <div *ngIf="isSelected(course.id)" class="mt-3 rounded-md bg-slate-50 p-2">
              <label class="text-xs font-semibold uppercase tracking-wide text-slate-700">Jornada</label>
              <select
                class="mt-1 w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
                [value]="selectedShift(course.id)"
                (change)="updateCourseShift(course.id, $any($event.target).value)"
              >
                <option value="Saturday">Sábado</option>
                <option value="Sunday">Domingo</option>
              </select>
            </div>
          </label>
        </div>

        <button
          class="btn-primary mt-4 w-full px-4 py-2 disabled:cursor-not-allowed disabled:opacity-60"
          [disabled]="selectedTotal === 0 || !isDistributionValid"
          (click)="createEnrollment()"
        >
          Generar asignación + orden de pago
        </button>
      </article>

      <article class="panel p-6">
        <h2 class="section-title text-lg">Cursos Atrasados Detectados</h2>
        <ul class="mt-4 space-y-2">
          <li *ngFor="let item of overdue" class="rounded-lg border border-slate-200 p-3">{{ item.code }} - {{ item.name }}</li>
        </ul>
        <p *ngIf="overdue.length === 0" class="mt-3 text-sm text-muted">No tienes cursos atrasados actualmente.</p>

        <section class="mt-6 rounded-lg border border-slate-200 bg-slate-50 p-4">
          <h3 class="text-sm font-bold uppercase tracking-wide text-[color:var(--umg-navy-700)]">Resumen de carga</h3>
          <p class="mt-2 text-sm text-slate-700">Plan principal: {{ shiftLabel(primaryShift) }}</p>
          <p class="mt-1 text-sm text-slate-700">Total seleccionados: {{ selectedTotal }} / {{ maxCourses }}</p>
          <p class="mt-1 text-sm text-slate-700">Sábado: {{ saturdayCount }} · Domingo: {{ sundayCount }}</p>
          <p class="mt-2 text-sm" [class.text-emerald-700]="isDistributionValid" [class.text-rose-700]="!isDistributionValid">
            {{ distributionMessage }}
          </p>
        </section>

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
            <tr *ngFor="let item of enrollments; trackBy: trackByEnrollmentId" class="border-b border-slate-100">
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
  readonly maxCourses = 6;

  courses: CourseDto[] = [];
  coursesByCycle = new Map<number, CourseDto[]>();
  cycleOrder: number[] = [];
  selectedCycle: number | null = null;
  preferredCycle: number | null = null;
  visibleCourses: CourseDto[] = [];
  overdue: OverdueCourseDto[] = [];
  enrollments: EnrollmentSummaryResponse[] = [];
  selectedCourseShifts = new Map<number, ShiftName>();
  primaryShift: ShiftName = 'Saturday';
  message = '';
  error = '';

  constructor() {
    this.loadProfile();
    this.loadPensum();
    this.loadOverdue();
    this.loadEnrollments();
  }

  get selectedTotal(): number {
    return this.selectedCourseShifts.size;
  }

  get saturdayCount(): number {
    return Array.from(this.selectedCourseShifts.values()).filter((shift) => shift === 'Saturday').length;
  }

  get sundayCount(): number {
    return Array.from(this.selectedCourseShifts.values()).filter((shift) => shift === 'Sunday').length;
  }

  get isDistributionValid(): boolean {
    if (this.selectedTotal === 0) {
      return true;
    }

    if (this.selectedTotal > this.maxCourses) {
      return false;
    }

    if (this.primaryShift === 'Saturday') {
      return this.saturdayCount > this.sundayCount;
    }

    return this.sundayCount > this.saturdayCount;
  }

  get distributionMessage(): string {
    if (this.selectedTotal === 0) {
      return 'Selecciona cursos para validar la distribución.';
    }

    if (this.selectedTotal > this.maxCourses) {
      return `Exceso de carga: máximo ${this.maxCourses} cursos por solicitud.`;
    }

    if (this.isDistributionValid) {
      return 'Distribución válida según tu plan.';
    }

    return this.primaryShift === 'Saturday'
      ? 'Distribución inválida: debes llevar mayoría de cursos en sábado.'
      : 'Distribución inválida: debes llevar mayoría de cursos en domingo.';
  }

  loadProfile(): void {
    this.http.get<ApiEnvelope<MeResponse>>(`${this.baseUrl}/me`).subscribe((response) => {
      if (!response.success) {
        return;
      }

      this.primaryShift = this.normalizeShift(response.data.shiftName);
      this.preferredCycle = this.normalizeCycle(response.data.currentCycle);
      this.applyPreferredCycle();
    });
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

  setCycle(cycle: number): void {
    this.selectedCycle = cycle;
    this.refreshVisibleCourses();
  }

  isSelected(courseId: number): boolean {
    return this.selectedCourseShifts.has(courseId);
  }

  selectedShift(courseId: number): ShiftName {
    return this.selectedCourseShifts.get(courseId) ?? this.primaryShift;
  }

  toggleCourse(courseId: number): void {
    const course = this.courses.find((item) => item.id === courseId);
    if (!course) {
      return;
    }

    if (!this.isSelectableCourse(course)) {
      this.error = this.nonSelectableReason(course);
      return;
    }

    if (this.selectedCourseShifts.has(courseId)) {
      this.selectedCourseShifts.delete(courseId);
      return;
    }

    if (this.selectedCourseShifts.size >= this.maxCourses) {
      this.error = `No puedes seleccionar más de ${this.maxCourses} cursos.`;
      return;
    }

    this.error = '';
    this.selectedCourseShifts.set(courseId, this.primaryShift);
  }

  updateCourseShift(courseId: number, shift: string): void {
    if (!this.selectedCourseShifts.has(courseId)) {
      return;
    }

    this.selectedCourseShifts.set(courseId, this.normalizeShift(shift));
  }

  shiftLabel(shift: ShiftName): string {
    return shift === 'Saturday' ? 'Sábado' : 'Domingo';
  }

  createEnrollment(): void {
    this.message = '';
    this.error = '';

    if (!this.isDistributionValid) {
      this.error = this.distributionMessage;
      return;
    }

    const payload: CreateEnrollmentRequest = {
      courseSelections: Array.from(this.selectedCourseShifts.entries()).map(([courseId, shift]) => ({ courseId, shift }))
    };

    this.http
      .post<ApiEnvelope<{ paymentOrderId: string; totalAmount: number; currency: string }>>(`${this.baseUrl}/enrollments`, payload)
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo crear la asignación.';
            return;
          }

          this.message = `Asignación creada. Orden ${response.data.paymentOrderId} - Total Q${response.data.totalAmount.toFixed(2)} ${response.data.currency}`;
          this.selectedCourseShifts.clear();
          this.loadEnrollments();
        },
        error: (error: HttpErrorResponse) => {
          this.error = this.extractErrorMessage(error);
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
        this.error = this.extractErrorMessage(error);
      }
    });
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
    if (this.selectedCycle === null || !map.has(this.selectedCycle)) {
      this.selectedCycle = this.cycleOrder[0] ?? null;
    }

    this.applyPreferredCycle();
    this.pruneSelection();
    this.refreshVisibleCourses();
  }

  private applyPreferredCycle(): void {
    if (this.preferredCycle === null || this.cycleOrder.length === 0) {
      return;
    }

    if (this.cycleOrder.includes(this.preferredCycle)) {
      this.selectedCycle = this.preferredCycle;
    } else {
      this.selectedCycle = this.cycleOrder[this.cycleOrder.length - 1] ?? this.selectedCycle;
    }

    this.preferredCycle = null;
    this.refreshVisibleCourses();
  }

  private refreshVisibleCourses(): void {
    if (this.selectedCycle === null) {
      this.visibleCourses = [];
      return;
    }

    this.visibleCourses = this.coursesByCycle.get(this.selectedCycle) ?? [];
  }

  private pruneSelection(): void {
    const validIds = new Set(this.courses.map((course) => course.id));
    Array.from(this.selectedCourseShifts.keys())
      .filter((courseId) => !validIds.has(courseId))
      .forEach((courseId) => this.selectedCourseShifts.delete(courseId));
  }

  trackByCycle(_: number, cycle: number): number {
    return cycle;
  }

  trackByCourseId(_: number, course: CourseDto): number {
    return course.id;
  }

  trackByEnrollmentId(_: number, enrollment: EnrollmentSummaryResponse): string {
    return enrollment.enrollmentId;
  }

  isSelectableCourse(course: CourseDto): boolean {
    return !course.isApproved;
  }

  private normalizeShift(shift: string | undefined | null): ShiftName {
    const value = (shift ?? '').trim().toLowerCase();
    if (value === 'sunday' || value === 'domingo') {
      return 'Sunday';
    }

    return 'Saturday';
  }

  private normalizeCycle(cycle: number | undefined | null): number | null {
    if (typeof cycle !== 'number' || Number.isNaN(cycle)) {
      return null;
    }

    const normalized = Math.trunc(cycle);
    if (normalized < 1) {
      return 1;
    }

    if (normalized > 10) {
      return 10;
    }

    return normalized;
  }

  private nonSelectableReason(course: CourseDto): string {
    if (course.isApproved) {
      return 'Este curso ya está aprobado.';
    }

    return 'Este curso no puede asignarse en este momento.';
  }

  private extractErrorMessage(error: HttpErrorResponse): string {
    const explicitMessage = error.error?.error?.message ?? error.error?.message;
    if (explicitMessage) {
      return explicitMessage;
    }

    const validationErrors = error.error?.validationErrors as Record<string, string[]> | undefined;
    if (validationErrors) {
      const firstKey = Object.keys(validationErrors)[0];
      if (firstKey && validationErrors[firstKey]?.length) {
        return validationErrors[firstKey][0];
      }
    }

    return 'Error de conexión.';
  }
}
