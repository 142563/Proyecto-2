import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { ApiEnvelope, CertificateSummaryResponse } from '../../shared/models/api.models';

@Component({
  standalone: true,
  selector: 'app-certificates-page',
  imports: [CommonModule, FormsModule, StatusBadgeComponent],
  template: `
    <section class="panel p-6">
      <h2 class="section-title text-lg">Certificación Digital</h2>

      <div class="mt-4 space-y-3">
        <input class="input-control" [(ngModel)]="purpose" placeholder="Motivo del certificado" />
        <button class="btn-primary px-4 py-2" (click)="requestCertificate()">Solicitar (genera orden)</button>
      </div>

      <div class="mt-5 border-t border-slate-200 pt-4" *ngIf="certificateId">
        <p class="text-sm text-muted">Certificado: {{ certificateId }}</p>
        <p class="text-sm text-muted">Código: {{ verificationCode }}</p>
        <button class="btn-primary mt-3 px-4 py-2" (click)="generateCertificate()">Generar PDF (si el pago está confirmado)</button>
        <button class="btn-secondary ml-2 mt-3 px-4 py-2" (click)="downloadCertificate()">Descargar PDF</button>
      </div>

      <div class="mt-5 border-t border-slate-200 pt-4">
        <h3 class="font-semibold">Verificar certificado</h3>
        <div class="mt-2 flex gap-2">
          <input class="input-control" [(ngModel)]="verifyCode" placeholder="Código de verificación" />
          <button class="btn-secondary px-4 py-2" (click)="verify()">Verificar</button>
        </div>
      </div>

      <p *ngIf="message" class="mt-3 text-sm text-emerald-700">{{ message }}</p>
      <p *ngIf="error" class="mt-2 text-sm text-rose-700">{{ error }}</p>
    </section>

    <section class="panel mt-6 p-6">
      <h3 class="section-title text-lg">Mis certificaciones</h3>
      <div class="mt-4 overflow-x-auto">
        <table class="table-clean w-full text-left text-sm">
          <thead>
            <tr class="border-b border-slate-200">
              <th class="py-2">Fecha</th>
              <th>Motivo</th>
              <th>Monto</th>
              <th>Vence</th>
              <th>Estado</th>
              <th class="text-right">Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of certificates" class="border-b border-slate-100">
              <td class="py-2">{{ item.createdAt | date:'short' }}</td>
              <td>{{ item.purpose }}</td>
              <td>Q{{ item.amount | number:'1.2-2' }} {{ item.currency }}</td>
              <td>{{ item.paymentExpiresAt | date:'short' }}</td>
              <td><app-status-badge [label]="item.status"></app-status-badge></td>
              <td class="text-right">
                <button *ngIf="item.status === 'Requested'" class="btn-danger px-3 py-1 text-xs" (click)="cancelCertificate(item.id)">
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
export class CertificatesPage {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  certificates: CertificateSummaryResponse[] = [];
  purpose = 'Certificado de estudios';
  certificateId = '';
  verificationCode = '';
  verifyCode = '';
  message = '';
  error = '';

  constructor() {
    this.loadMyCertificates();
  }

  loadMyCertificates(): void {
    this.http.get<ApiEnvelope<CertificateSummaryResponse[]>>(`${this.baseUrl}/certificates/my`).subscribe((response) => {
      this.certificates = response.success ? response.data : [];
    });
  }

  requestCertificate(): void {
    this.error = '';
    this.message = '';
    this.http
      .post<ApiEnvelope<{ certificateId: string; paymentOrderId: string; amount: number; currency: string; verificationCode: string }>>(
        `${this.baseUrl}/certificates`,
        { purpose: this.purpose }
      )
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No fue posible crear el certificado.';
            return;
          }

          this.certificateId = response.data.certificateId;
          this.verificationCode = response.data.verificationCode;
          this.message = `Solicitud creada. Orden de pago: ${response.data.paymentOrderId} por Q${response.data.amount.toFixed(2)} ${response.data.currency}`;
          this.loadMyCertificates();
        },
        error: (error: HttpErrorResponse) => {
          this.error = error.error?.error?.message ?? 'Error de conexión.';
        }
      });
  }

  generateCertificate(): void {
    if (!this.certificateId) {
      return;
    }

    this.http
      .post<ApiEnvelope<{ status: string }>>(`${this.baseUrl}/certificates/${this.certificateId}/generate`, {
        sendEmail: true,
        includeQr: false
      })
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo generar el PDF.';
            return;
          }

          this.message = `Certificado generado con estado ${response.data.status}.`;
          this.loadMyCertificates();
        },
        error: (error: HttpErrorResponse) => {
          this.error = error.error?.error?.message ?? 'Error de conexión.';
        }
      });
  }

  downloadCertificate(): void {
    if (!this.certificateId) {
      return;
    }

    this.http.get(`${this.baseUrl}/certificates/${this.certificateId}/download`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `certificate-${this.certificateId}.pdf`;
        anchor.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.error = 'No fue posible descargar el certificado.';
      }
    });
  }

  cancelCertificate(certificateId: string): void {
    this.error = '';
    this.message = '';

    this.http.post<ApiEnvelope<{ certificateId: string }>>(`${this.baseUrl}/certificates/${certificateId}/cancel`, {}).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No fue posible cancelar el certificado.';
          return;
        }

        this.message = 'Solicitud de certificación cancelada.';
        this.loadMyCertificates();
      },
      error: (error: HttpErrorResponse) => {
        this.error = error.error?.error?.message ?? 'Error de conexión.';
      }
    });
  }

  verify(): void {
    this.http.get<ApiEnvelope<{ message: string }>>(`${this.baseUrl}/certificates/verify/${this.verifyCode}`).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No se pudo verificar.';
          return;
        }

        this.message = response.data.message;
      },
      error: (error: HttpErrorResponse) => {
        this.error = error.error?.error?.message ?? 'Error de conexión.';
      }
    });
  }
}
