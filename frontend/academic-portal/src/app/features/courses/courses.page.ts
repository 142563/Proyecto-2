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
    <section class="grid gap-6 xl:grid-cols-12">
      <article class="panel p-6 xl:col-span-8">
        <header>
          <p class="text-xs font-semibold uppercase tracking-[0.13em] text-[color:var(--umg-navy-700)]">Flujo de asignacion academica</p>
          <h2 class="section-title mt-2 text-2xl">Cursos del Pensum</h2>
          <p class="mt-1 text-sm text-muted">
            1. Selecciona ciclo, 2. Elige cursos no aprobados, 3. Define jornada por curso, 4. Genera solicitud y orden de pago.
          </p>
        </header>

        <section class="mt-5 rounded-xl border border-slate-200 bg-slate-50/70 p-4">
          <label class="text-xs font-semibold uppercase tracking-[0.08em] text-[color:var(--umg-navy-700)]">Selecciona ciclo</label>
          <div class="mt-3 flex flex-wrap gap-2">
            <button
              *ngFor="let cycle of cycleOrder; trackBy: trackByCycle"
              type="button"
              class="rounded-full border px-3.5 py-1.5 text-sm font-semibold transition"
              [ngClass]="
                selectedCycle === cycle
                  ? 'border-[color:var(--umg-navy-900)] bg-[color:var(--umg-navy-900)] text-white shadow-[0_8px_18px_rgba(10,35,64,0.22)]'
                  : 'border-slate-300 bg-white text-slate-700 hover:border-slate-400 hover:bg-slate-100'
              "
              (click)="setCycle(cycle)"
            >
              Ciclo {{ cycle }}
            </button>
          </div>
        </section>

        <section class="mt-5">
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-sm font-semibold uppercase tracking-[0.08em] text-slate-600">Cursos disponibles</h3>
            <span class="rounded-full border border-slate-200 bg-slate-50 px-3 py-1 text-xs font-semibold text-slate-600">
              Seleccionados: {{ selectedTotal }}/{{ maxCourses }}
            </span>
          </div>

          <div *ngIf="visibleCourses.length === 0" class="rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-10 text-center">
            <p class="font-display text-lg font-bold text-[color:var(--umg-navy-900)]">No hay cursos en este ciclo</p>
            <p class="mt-1 text-sm text-muted">Selecciona otro ciclo para visualizar cursos del pensum.</p>
          </div>

          <div *ngIf="visibleCourses.length > 0" class="space-y-3">
            <article
              *ngFor="let course of visibleCourses; trackBy: trackByCourseId"
              class="rounded-xl border border-slate-200 bg-white p-4 transition"
              [ngClass]="{
                'cursor-pointer hover:border-slate-300 hover:shadow-[0_10px_22px_rgba(10,35,64,0.08)]': isSelectableCourse(course),
                'border-[color:var(--umg-navy-700)] bg-sky-50/60 shadow-[0_12px_24px_rgba(12,53,95,0.08)]': isSelected(course.id)
              }"
              (click)="onCourseCardClick(course)"
            >
              <div class="flex items-start justify-between gap-3">
                <div class="min-w-0">
                  <p class="font-display text-lg font-bold text-[color:var(--umg-navy-900)]">{{ course.code }} - {{ course.name }}</p>
                  <div class="mt-2 flex flex-wrap gap-2">
                    <span class="rounded-full border border-slate-200 bg-slate-50 px-2.5 py-0.5 text-xs font-semibold text-slate-600">
                      {{ course.credits }} creditos
                    </span>
                    <span class="rounded-full border border-slate-200 bg-slate-50 px-2.5 py-0.5 text-xs font-semibold text-slate-600">
                      {{ course.hoursPerWeek }} h/semana
                    </span>
                    <span class="rounded-full border border-slate-200 bg-slate-50 px-2.5 py-0.5 text-xs font-semibold text-slate-600">
                      {{ course.hoursTotal }} h totales
                    </span>
                    <span *ngIf="course.isLab" class="rounded-full border border-cyan-200 bg-cyan-50 px-2.5 py-0.5 text-xs font-semibold text-cyan-700">
                      Laboratorio
                    </span>
                    <span *ngIf="course.isOverdue" class="rounded-full border border-amber-200 bg-amber-50 px-2.5 py-0.5 text-xs font-semibold text-amber-700">
                      Atrasado
                    </span>
                    <span *ngIf="course.isApproved" class="rounded-full border border-emerald-200 bg-emerald-50 px-2.5 py-0.5 text-xs font-semibold text-emerald-700">
                      Aprobado
                    </span>
                  </div>
                  <p class="mt-2 text-xs text-muted">
                    <span class="font-semibold text-slate-600">Prerrequisitos:</span> {{ course.prerequisiteSummary }}
                  </p>
                </div>

                <div class="pt-1">
                  <input
                    type="checkbox"
                    class="h-4 w-4 rounded border-slate-300 text-[color:var(--umg-navy-700)] focus:ring-[color:var(--umg-navy-700)]"
                    [checked]="isSelected(course.id)"
                    [disabled]="!isSelectableCourse(course)"
                    (click)="$event.stopPropagation()"
                    (change)="toggleCourse(course.id)"
                  />
                </div>
              </div>

              <div *ngIf="isSelected(course.id)" class="mt-3 rounded-lg border border-slate-200 bg-white p-3" (click)="$event.stopPropagation()">
                <label class="text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Jornada del curso</label>
                <select
                  class="input-control mt-1.5 text-sm"
                  [value]="selectedShift(course.id)"
                  (change)="updateCourseShift(course.id, $any($event.target).value)"
                >
                  <option value="Saturday">Sabado</option>
                  <option value="Sunday">Domingo</option>
                </select>
              </div>
            </article>
          </div>
        </section>

        <button
          class="btn-primary mt-5 inline-flex w-full items-center justify-center gap-2 px-4 py-3 text-sm font-bold disabled:cursor-not-allowed disabled:opacity-60"
          [disabled]="selectedTotal === 0 || !isDistributionValid"
          (click)="createEnrollment()"
        >
          Generar asignacion + orden de pago
        </button>
      </article>

      <aside class="panel p-6 xl:col-span-4">
        <section>
          <div class="mb-3 flex items-center justify-between">
            <h3 class="section-title text-lg">Cursos atrasados detectados</h3>
            <span class="rounded-full border border-amber-200 bg-amber-50 px-2.5 py-0.5 text-xs font-semibold text-amber-700">
              {{ overdue.length }}
            </span>
          </div>

          <div *ngIf="overdue.length === 0" class="rounded-xl border border-dashed border-slate-300 bg-slate-50 px-4 py-8 text-center">
            <p class="font-semibold text-slate-700">Sin cursos atrasados</p>
            <p class="mt-1 text-sm text-muted">Tu historial no muestra cursos pendientes reprobados.</p>
          </div>

          <ul *ngIf="overdue.length > 0" class="space-y-2.5">
            <li *ngFor="let item of overdue" class="rounded-xl border border-slate-200 bg-white px-3 py-2.5">
              <p class="font-semibold text-[color:var(--umg-navy-900)]">{{ item.code }}</p>
              <p class="text-sm text-slate-700">{{ item.name }}</p>
            </li>
          </ul>
        </section>

        <section class="mt-5 rounded-xl border border-slate-200 bg-slate-50 p-4">
          <h3 class="text-sm font-bold uppercase tracking-[0.08em] text-[color:var(--umg-navy-700)]">Resumen de carga</h3>
          <div class="mt-3 grid grid-cols-2 gap-2">
            <div class="rounded-lg border border-slate-200 bg-white px-3 py-2">
              <p class="text-[11px] font-semibold uppercase tracking-[0.08em] text-slate-500">Plan principal</p>
              <p class="mt-1 font-semibold text-[color:var(--umg-navy-900)]">{{ shiftLabel(primaryShift) }}</p>
            </div>
            <div class="rounded-lg border border-slate-200 bg-white px-3 py-2">
              <p class="text-[11px] font-semibold uppercase tracking-[0.08em] text-slate-500">Total</p>
              <p class="mt-1 font-semibold text-[color:var(--umg-navy-900)]">{{ selectedTotal }} / {{ maxCourses }}</p>
            </div>
            <div class="rounded-lg border border-slate-200 bg-white px-3 py-2">
              <p class="text-[11px] font-semibold uppercase tracking-[0.08em] text-slate-500">Sabado</p>
              <p class="mt-1 font-semibold text-[color:var(--umg-navy-900)]">{{ saturdayCount }}</p>
            </div>
            <div class="rounded-lg border border-slate-200 bg-white px-3 py-2">
              <p class="text-[11px] font-semibold uppercase tracking-[0.08em] text-slate-500">Domingo</p>
              <p class="mt-1 font-semibold text-[color:var(--umg-navy-900)]">{{ sundayCount }}</p>
            </div>
          </div>
          <p class="mt-3 text-sm" [class.text-emerald-700]="isDistributionValid" [class.text-rose-700]="!isDistributionValid">
            {{ distributionMessage }}
          </p>
        </section>

        <div class="mt-4 space-y-2">
          <p *ngIf="message" class="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{{ message }}</p>
          <p *ngIf="error" class="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">{{ error }}</p>
        </div>
      </aside>
    </section>

    <section class="panel mt-6 p-6">
      <div class="mb-4 flex items-center justify-between">
        <h3 class="section-title text-lg">Mis solicitudes de asignacion</h3>
        <span class="rounded-full border border-slate-200 bg-slate-50 px-3 py-1 text-xs font-semibold text-slate-600">
          {{ enrollments.length }} registro(s)
        </span>
      </div>

      <div *ngIf="enrollments.length === 0" class="rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-10 text-center">
        <p class="font-display text-lg font-bold text-[color:var(--umg-navy-900)]">Aun no tienes solicitudes</p>
        <p class="mt-1 text-sm text-muted">Cuando generes una asignacion, aparecera aqui con su estado y acciones.</p>
      </div>

      <div *ngIf="enrollments.length > 0" class="overflow-x-auto rounded-xl border border-slate-200">
        <table class="table-clean w-full text-left text-sm">
          <thead class="bg-slate-50">
            <tr class="border-b border-slate-200">
              <th class="py-3 pl-4">Fecha</th>
              <th class="py-3">Tipo</th>
              <th class="py-3">Total</th>
              <th class="py-3">Vence</th>
              <th class="py-3">Estado</th>
              <th class="py-3 pr-4 text-right">Accion</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of enrollments; trackBy: trackByEnrollmentId" class="border-b border-slate-100 transition hover:bg-slate-50/70">
              <td class="py-3 pl-4">{{ item.createdAt | date:'short' }}</td>
              <td class="py-3">{{ item.enrollmentType }}</td>
              <td class="py-3 font-semibold text-[color:var(--umg-navy-900)]">Q{{ item.totalAmount | number:'1.2-2' }} {{ item.currency }}</td>
              <td class="py-3">{{ item.paymentExpiresAt | date:'short' }}</td>
              <td class="py-3"><app-status-badge [label]="item.status"></app-status-badge></td>
              <td class="py-3 pr-4 text-right">
                <button
                  *ngIf="item.status === 'Confirmed'"
                  class="btn-secondary mr-2 px-3 py-1.5 text-xs"
                  (click)="downloadDire(item.enrollmentId)"
                >
                  Descargar DIRE
                </button>
                <button
                  *ngIf="item.status === 'PendingPayment'"
                  class="btn-danger px-3 py-1.5 text-xs"
                  (click)="cancelEnrollment(item.enrollmentId)"
                >
                  Cancelar
                </button>
                <span *ngIf="item.status !== 'Confirmed' && item.status !== 'PendingPayment'" class="text-xs text-slate-400">Sin accion</span>
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
      return 'Selecciona cursos para validar la distribucion.';
    }

    if (this.selectedTotal > this.maxCourses) {
      return `Exceso de carga: maximo ${this.maxCourses} cursos por solicitud.`;
    }

    if (this.isDistributionValid) {
      return 'Distribucion valida segun tu plan.';
    }

    return this.primaryShift === 'Saturday'
      ? 'Distribucion invalida: debes llevar mayoria de cursos en sabado.'
      : 'Distribucion invalida: debes llevar mayoria de cursos en domingo.';
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

  onCourseCardClick(course: CourseDto): void {
    if (!this.isSelectableCourse(course)) {
      return;
    }

    this.toggleCourse(course.id);
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
      this.error = `No puedes seleccionar mas de ${this.maxCourses} cursos.`;
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
    return shift === 'Saturday' ? 'Sabado' : 'Domingo';
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
            this.error = response.error?.message ?? 'No se pudo crear la asignacion.';
            return;
          }

          this.message = `Asignacion creada. Orden ${response.data.paymentOrderId} - Total Q${response.data.totalAmount.toFixed(2)} ${response.data.currency}`;
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
          this.error = response.error?.message ?? 'No se pudo cancelar la asignacion.';
          return;
        }

        this.message = 'Asignacion cancelada correctamente.';
        this.loadEnrollments();
      },
      error: (error: HttpErrorResponse) => {
        this.error = this.extractErrorMessage(error);
      }
    });
  }

  downloadDire(enrollmentId: string): void {
    this.error = '';

    this.http.get(`${this.baseUrl}/enrollments/${enrollmentId}/dire`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `dire-inscripcion-${enrollmentId}.pdf`;
        anchor.click();
        window.URL.revokeObjectURL(url);
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
      return 'Este curso ya esta aprobado.';
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

    return 'Error de conexion.';
  }
}
