import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({
  standalone: true,
  selector: 'app-certificates-page',
  imports: [CommonModule, FormsModule],
  template: `
    <section class="rounded-2xl bg-white p-6 shadow-sm">
      <h2 class="text-lg font-bold">Certificacion Digital</h2>

      <div class="mt-4 space-y-3">
        <input
          class="w-full rounded-lg border border-slate-300 px-3 py-2"
          [(ngModel)]="purpose"
          placeholder="Motivo del certificado"
        />
        <button class="rounded-lg bg-slate-900 px-4 py-2 text-white" (click)="requestCertificate()">
          Solicitar (genera orden)
        </button>
      </div>

      <div class="mt-5 border-t pt-4" *ngIf="certificateId">
        <p class="text-sm text-slate-600">Certificado: {{ certificateId }}</p>
        <p class="text-sm text-slate-600">Codigo: {{ verificationCode }}</p>
        <button class="mt-3 rounded-lg bg-emerald-700 px-4 py-2 text-white" (click)="generateCertificate()">
          Generar PDF (si pago esta confirmado)
        </button>
        <button class="ml-2 mt-3 rounded-lg bg-indigo-700 px-4 py-2 text-white" (click)="downloadCertificate()">
          Descargar PDF
        </button>
      </div>

      <div class="mt-5 border-t pt-4">
        <h3 class="font-semibold">Verificar certificado</h3>
        <div class="mt-2 flex gap-2">
          <input
            class="w-full rounded-lg border border-slate-300 px-3 py-2"
            [(ngModel)]="verifyCode"
            placeholder="Codigo de verificacion"
          />
          <button class="rounded-lg bg-slate-700 px-4 py-2 text-white" (click)="verify()">Verificar</button>
        </div>
      </div>

      <p *ngIf="message" class="mt-3 text-sm text-emerald-700">{{ message }}</p>
      <p *ngIf="error" class="mt-2 text-sm text-rose-700">{{ error }}</p>
    </section>
  `
})
export class CertificatesPage {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  purpose = 'Certificado de estudios';
  certificateId = '';
  verificationCode = '';
  verifyCode = '';
  message = '';
  error = '';

  requestCertificate(): void {
    this.error = '';
    this.message = '';
    this.http.post<any>(`${this.baseUrl}/certificates`, { purpose: this.purpose }).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No fue posible crear el certificado.';
          return;
        }

        this.certificateId = response.data.certificateId;
        this.verificationCode = response.data.verificationCode;
        this.message = `Solicitud creada. Orden de pago: ${response.data.paymentOrderId}`;
      },
      error: () => {
        this.error = 'Error de conexion.';
      }
    });
  }

  generateCertificate(): void {
    if (!this.certificateId) {
      return;
    }

    this.http
      .post<any>(`${this.baseUrl}/certificates/${this.certificateId}/generate`, {
        sendEmail: true,
        includeQr: false
      })
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo generar el PDF.';
            return;
          }

          this.message = `Certificado generado con estado ${response.data.status}`;
        },
        error: () => {
          this.error = 'Error de conexion.';
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

  verify(): void {
    this.http.get<any>(`${this.baseUrl}/certificates/verify/${this.verifyCode}`).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No se pudo verificar.';
          return;
        }

        this.message = response.data.message;
      },
      error: () => {
        this.error = 'Error de conexion.';
      }
    });
  }
}
