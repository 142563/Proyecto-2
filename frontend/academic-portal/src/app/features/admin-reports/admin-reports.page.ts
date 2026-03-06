import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { API_BASE_URL } from '../../core/config/api.config';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { ApiEnvelope } from '../../shared/models/api.models';

@Component({
  standalone: true,
  selector: 'app-admin-reports-page',
  imports: [CommonModule, StatusBadgeComponent],
  template: `
    <section class="panel p-6">
      <h2 class="section-title text-lg">Panel Administrativo UMG</h2>
      <div class="mt-4 flex flex-wrap gap-2">
        <button class="btn-primary px-3 py-2 text-sm" (click)="loadReport('transfers')">Traslados</button>
        <button class="btn-primary px-3 py-2 text-sm" (click)="loadReport('enrollments')">Asignaciones</button>
        <button class="btn-primary px-3 py-2 text-sm" (click)="loadReport('certificates')">Certificaciones</button>
      </div>

      <div class="mt-3 flex flex-wrap gap-2">
        <button class="btn-secondary px-3 py-2 text-sm" (click)="export('pdf')">Exportar PDF</button>
        <button class="btn-secondary px-3 py-2 text-sm" (click)="export('xlsx')">Exportar Excel</button>
      </div>

      <p *ngIf="message" class="mt-3 text-sm text-emerald-700">{{ message }}</p>
      <p *ngIf="error" class="mt-2 text-sm text-rose-700">{{ error }}</p>
    </section>

    <section class="panel mt-6 p-6" *ngIf="reportType !== 'transfers'">
      <h3 class="section-title text-lg">Resultado: {{ reportType }}</h3>
      <div class="mt-4 overflow-x-auto">
        <table class="table-clean w-full text-left text-sm">
          <thead>
            <tr class="border-b border-slate-200">
              <th class="py-2" *ngFor="let key of keys">{{ key }}</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let row of rows" class="border-b border-slate-100">
              <td class="py-2" *ngFor="let key of keys">{{ row[key] }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="panel mt-6 p-6" *ngIf="reportType === 'transfers'">
      <h3 class="section-title text-lg">Revisión de traslados</h3>
      <div class="mt-4 overflow-x-auto">
        <table class="table-clean w-full text-left text-sm">
          <thead>
            <tr class="border-b border-slate-200">
              <th class="py-2">Estudiante</th>
              <th>Origen</th>
              <th>Destino</th>
              <th>Jornada</th>
              <th>Estado</th>
              <th class="text-right">Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let row of rows" class="border-b border-slate-100">
              <td class="py-2">{{ row.studentName }} ({{ row.studentCode }})</td>
              <td>{{ row.fromCampus }}</td>
              <td>{{ row.toCampus }}</td>
              <td>{{ row.shift }}</td>
              <td><app-status-badge [label]="row.status"></app-status-badge></td>
              <td class="text-right">
                <div class="inline-flex gap-2" *ngIf="row.status === 'PendingReview'">
                  <button class="btn-primary px-3 py-1 text-xs" (click)="reviewTransfer(row.transferId, 'Approved')">Aprobar</button>
                  <button class="btn-danger px-3 py-1 text-xs" (click)="reviewTransfer(row.transferId, 'Rejected')">Rechazar</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class AdminReportsPage {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  reportType: 'transfers' | 'enrollments' | 'certificates' = 'transfers';
  rows: any[] = [];
  keys: string[] = [];
  error = '';
  message = '';

  constructor() {
    this.loadReport('transfers');
  }

  loadReport(type: 'transfers' | 'enrollments' | 'certificates'): void {
    this.reportType = type;
    this.error = '';
    this.message = '';

    this.http.get<ApiEnvelope<any[]>>(`${this.baseUrl}/reports/${type}`).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No se pudo cargar el reporte.';
          return;
        }

        this.rows = response.data ?? [];
        this.keys = this.rows.length ? Object.keys(this.rows[0]) : [];
      },
      error: (error: HttpErrorResponse) => {
        this.error = error.error?.error?.message ?? 'Error de conexión.';
      }
    });
  }

  reviewTransfer(transferId: string, decision: 'Approved' | 'Rejected'): void {
    const notes = prompt(`Notas de revisión (${decision}):`) ?? '';
    this.http
      .post<ApiEnvelope<{ status: string }>>(`${this.baseUrl}/transfers/${transferId}/review`, {
        decision,
        notes
      })
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo registrar la revisión.';
            return;
          }

          this.message = `Traslado actualizado a estado ${response.data.status}.`;
          this.loadReport('transfers');
        },
        error: (error: HttpErrorResponse) => {
          this.error = error.error?.error?.message ?? 'Error de conexión.';
        }
      });
  }

  export(format: 'pdf' | 'xlsx'): void {
    this.http
      .get(`${this.baseUrl}/reports/${this.reportType}/export`, {
        params: { format },
        responseType: 'blob'
      })
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `${this.reportType}-report.${format}`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          this.error = 'No se pudo exportar el archivo.';
        }
      });
  }
}
