import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({
  standalone: true,
  selector: 'app-admin-reports-page',
  imports: [CommonModule],
  template: `
    <section class="rounded-2xl bg-white p-6 shadow-sm">
      <h2 class="text-lg font-bold">Reportes Administrativos</h2>
      <div class="mt-4 flex flex-wrap gap-2">
        <button class="rounded-lg bg-slate-900 px-3 py-2 text-white" (click)="loadReport('transfers')">Traslados</button>
        <button class="rounded-lg bg-slate-900 px-3 py-2 text-white" (click)="loadReport('enrollments')">
          Asignaciones
        </button>
        <button class="rounded-lg bg-slate-900 px-3 py-2 text-white" (click)="loadReport('certificates')">
          Certificaciones
        </button>
      </div>

      <div class="mt-3 flex flex-wrap gap-2">
        <button class="rounded-lg bg-indigo-700 px-3 py-2 text-white" (click)="export('pdf')">Exportar PDF</button>
        <button class="rounded-lg bg-emerald-700 px-3 py-2 text-white" (click)="export('xlsx')">
          Exportar Excel
        </button>
      </div>

      <div class="mt-5 overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead>
            <tr class="border-b text-slate-500">
              <th class="py-2" *ngFor="let key of keys">{{ key }}</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let row of rows" class="border-b">
              <td class="py-2" *ngFor="let key of keys">{{ row[key] }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <p *ngIf="error" class="mt-3 text-sm text-rose-700">{{ error }}</p>
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

  loadReport(type: 'transfers' | 'enrollments' | 'certificates'): void {
    this.reportType = type;
    this.error = '';

    this.http.get<any>(`${this.baseUrl}/reports/${type}`).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No se pudo cargar el reporte.';
          return;
        }

        this.rows = response.data ?? [];
        this.keys = this.rows.length ? Object.keys(this.rows[0]) : [];
      },
      error: () => {
        this.error = 'Error de conexion.';
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
