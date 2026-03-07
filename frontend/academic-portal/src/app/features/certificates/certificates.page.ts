import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import {
  ApiEnvelope,
  CertificateSummaryResponse,
  CertificateTypeResponse,
  MockCheckoutRequest,
  MockCheckoutResponse
} from '../../shared/models/api.models';

@Component({
  standalone: true,
  selector: 'app-certificates-page',
  imports: [CommonModule, FormsModule, StatusBadgeComponent],
  template: `
    <section class="panel p-6">
      <h2 class="section-title text-lg">Certificación Digital</h2>

      <div class="mt-4 space-y-3">
        <label class="text-xs font-semibold uppercase tracking-wide text-[color:var(--umg-navy-700)]">Tipo de certificación</label>
        <select class="input-control" [(ngModel)]="selectedTypeCode">
          <option *ngFor="let type of certificateTypes" [value]="type.code">{{ type.name }}</option>
        </select>
        <p *ngIf="selectedType" class="text-sm text-muted">{{ selectedType.description }}</p>
        <p *ngIf="selectedType?.requiresFullPensum" class="text-sm text-amber-700">
          Requiere tener el pensum completo aprobado.
        </p>
        <button class="btn-primary px-4 py-2" [disabled]="!selectedTypeCode" (click)="requestCertificate()">
          Solicitar certificado
        </button>
      </div>

      <div class="mt-5 border-t border-slate-200 pt-4">
        <h3 class="font-semibold">Verificar certificado</h3>
        <div class="mt-2 flex gap-2">
          <input class="input-control" [(ngModel)]="verifyCode" placeholder="Código de verificación" />
          <button class="btn-secondary px-4 py-2" (click)="verify()">Verificar</button>
        </div>
      </div>

      <div *ngIf="checkoutCertificate" class="mt-5 rounded-lg border border-slate-200 bg-slate-50 p-4">
        <h3 class="text-sm font-semibold uppercase tracking-wide text-[color:var(--umg-navy-700)]">Pago con tarjeta (demo)</h3>
        <p class="mt-1 text-sm text-muted">
          Certificado: {{ checkoutCertificate.purpose }} · Q{{ checkoutCertificate.amount | number:'1.2-2' }} {{ checkoutCertificate.currency }}
        </p>

        <div class="mt-3 grid gap-3 md:grid-cols-2">
          <input class="input-control" [(ngModel)]="checkout.cardHolderName" placeholder="Nombre del titular" />
          <input class="input-control" [(ngModel)]="checkout.cardNumber" placeholder="Número de tarjeta" />
          <input class="input-control" type="number" [(ngModel)]="checkout.expiryMonth" placeholder="Mes (MM)" />
          <input class="input-control" type="number" [(ngModel)]="checkout.expiryYear" placeholder="Año (YYYY)" />
          <input class="input-control md:col-span-2" [(ngModel)]="checkout.cvv" placeholder="CVV" />
        </div>

        <div class="mt-3 flex gap-2">
          <button class="btn-primary px-4 py-2 text-sm" [disabled]="checkoutLoading" (click)="submitCheckout()">
            {{ checkoutLoading ? 'Procesando...' : 'Confirmar pago' }}
          </button>
          <button class="btn-secondary px-4 py-2 text-sm" [disabled]="checkoutLoading" (click)="cancelCheckout()">
            Cancelar
          </button>
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
              <th>Pago</th>
              <th>Estado</th>
              <th class="text-right">Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of certificates" class="border-b border-slate-100">
              <td class="py-2">{{ item.createdAt | date:'short' }}</td>
              <td>{{ item.purpose }}</td>
              <td>Q{{ item.amount | number:'1.2-2' }} {{ item.currency }}</td>
              <td><app-status-badge [label]="item.paymentStatus"></app-status-badge></td>
              <td><app-status-badge [label]="item.status"></app-status-badge></td>
              <td class="text-right">
                <div class="flex justify-end gap-2">
                  <button
                    *ngIf="item.status === 'Requested' && item.paymentStatus === 'Pending'"
                    class="btn-primary px-3 py-1 text-xs"
                    (click)="openCheckout(item)"
                  >
                    Pagar
                  </button>
                  <button
                    *ngIf="item.status === 'Requested' && item.paymentStatus === 'Paid'"
                    class="btn-secondary px-3 py-1 text-xs"
                    (click)="generateCertificate(item.id)"
                  >
                    Generar
                  </button>
                  <button
                    *ngIf="item.pdfAvailable"
                    class="btn-secondary px-3 py-1 text-xs"
                    (click)="downloadCertificate(item.id)"
                  >
                    Ver/Descargar
                  </button>
                  <button
                    *ngIf="item.status === 'Requested' && item.paymentStatus === 'Pending'"
                    class="btn-danger px-3 py-1 text-xs"
                    (click)="cancelCertificate(item.id)"
                  >
                    Cancelar
                  </button>
                </div>
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

  certificateTypes: CertificateTypeResponse[] = [];
  certificates: CertificateSummaryResponse[] = [];
  selectedTypeCode = '';
  verifyCode = '';
  checkoutCertificate: CertificateSummaryResponse | null = null;
  checkoutLoading = false;
  checkout: MockCheckoutRequest = {
    cardHolderName: 'Usuario Demo',
    cardNumber: '4242424242424242',
    expiryMonth: 12,
    expiryYear: 2032,
    cvv: '123'
  };
  message = '';
  error = '';

  constructor() {
    this.loadCertificateTypes();
    this.loadMyCertificates();
  }

  get selectedType(): CertificateTypeResponse | undefined {
    return this.certificateTypes.find((type) => type.code === this.selectedTypeCode);
  }

  loadCertificateTypes(): void {
    this.http.get<ApiEnvelope<CertificateTypeResponse[]>>(`${this.baseUrl}/certificates/types`).subscribe((response) => {
      if (!response.success) {
        return;
      }

      this.certificateTypes = response.data;
      if (!this.selectedTypeCode && this.certificateTypes.length > 0) {
        this.selectedTypeCode = this.certificateTypes[0].code;
      }
    });
  }

  loadMyCertificates(): void {
    this.http.get<ApiEnvelope<CertificateSummaryResponse[]>>(`${this.baseUrl}/certificates/my`).subscribe((response) => {
      this.certificates = response.success ? response.data : [];
    });
  }

  requestCertificate(): void {
    this.error = '';
    this.message = '';
    if (!this.selectedTypeCode) {
      this.error = 'Debes seleccionar un tipo de certificación.';
      return;
    }

    this.http
      .post<ApiEnvelope<{ certificateId: string; paymentOrderId: string; amount: number; currency: string; verificationCode: string }>>(
        `${this.baseUrl}/certificates`,
        { purpose: this.selectedTypeCode }
      )
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No fue posible crear el certificado.';
            return;
          }

          this.message = `Solicitud creada. Orden de pago: ${response.data.paymentOrderId} por Q${response.data.amount.toFixed(2)} ${response.data.currency}`;
          this.loadMyCertificates();
        },
        error: (error: HttpErrorResponse) => {
          this.error = this.extractErrorMessage(error);
        }
      });
  }

  generateCertificate(certificateId: string): void {
    this.error = '';
    this.message = '';

    this.http
      .post<ApiEnvelope<{ status: string }>>(`${this.baseUrl}/certificates/${certificateId}/generate`, {
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
          this.downloadCertificate(certificateId);
        },
        error: (error: HttpErrorResponse) => {
          this.error = this.extractErrorMessage(error);
        }
      });
  }

  downloadCertificate(certificateId: string): void {
    this.http.get(`${this.baseUrl}/certificates/${certificateId}/download`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `certificate-${certificateId}.pdf`;
        anchor.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.error = 'No fue posible descargar el certificado.';
      }
    });
  }

  openCheckout(certificate: CertificateSummaryResponse): void {
    this.error = '';
    this.message = '';
    this.checkoutCertificate = certificate;
  }

  cancelCheckout(): void {
    this.checkoutLoading = false;
    this.checkoutCertificate = null;
  }

  submitCheckout(): void {
    if (!this.checkoutCertificate || this.checkoutLoading) {
      return;
    }

    this.error = '';
    this.message = '';
    this.checkoutLoading = true;

    this.http
      .post<ApiEnvelope<MockCheckoutResponse>>(
        `${this.baseUrl}/payments/${this.checkoutCertificate.paymentOrderId}/mock-checkout`,
        this.checkout
      )
      .subscribe({
        next: (response) => {
          this.checkoutLoading = false;
          if (!response.success) {
            this.error = response.error?.message ?? 'No fue posible completar el pago.';
            return;
          }

          this.message = 'Pago aprobado correctamente.';
          const generatedCertificate = response.data.certificate;
          if (generatedCertificate?.pdfAvailable) {
            this.downloadCertificate(generatedCertificate.certificateId);
            this.message += ' Certificado generado y descargado.';
          }

          this.checkoutCertificate = null;
          this.loadMyCertificates();
        },
        error: (error: HttpErrorResponse) => {
          this.checkoutLoading = false;
          this.error = this.extractErrorMessage(error);
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
        this.error = this.extractErrorMessage(error);
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
        this.error = this.extractErrorMessage(error);
      }
    });
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
