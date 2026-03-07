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
      <header class="mb-5">
        <p class="text-xs font-semibold uppercase tracking-[0.12em] text-[color:var(--umg-navy-700)]">Tramite academico digital</p>
        <h2 class="section-title mt-1 text-2xl">Certificacion Digital</h2>
        <p class="mt-1 text-sm text-muted">Selecciona un tipo de certificacion, genera tu solicitud y da seguimiento desde el historial.</p>
      </header>

      <div class="grid gap-4 lg:grid-cols-12">
        <article class="rounded-xl border border-slate-200 bg-slate-50 p-4 lg:col-span-7">
          <p class="text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Nueva solicitud</p>

          <div class="mt-3 space-y-3">
            <label class="text-xs font-semibold uppercase tracking-[0.08em] text-[color:var(--umg-navy-700)]">Tipo de certificacion</label>
            <div class="relative">
              <select class="input-control pr-10" [(ngModel)]="selectedTypeCode">
                <option *ngFor="let type of certificateTypes" [value]="type.code">{{ type.name }}</option>
              </select>
              <span class="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-slate-400">
                <svg class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fill-rule="evenodd" d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.51a.75.75 0 01-1.08 0l-4.25-4.51a.75.75 0 01.02-1.06z" clip-rule="evenodd" />
                </svg>
              </span>
            </div>

            <p *ngIf="selectedType" class="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700">{{ selectedType.description }}</p>
            <p *ngIf="selectedType?.requiresFullPensum" class="rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-700">
              Requiere tener el pensum completo aprobado.
            </p>

            <button class="btn-primary w-full px-4 py-2.5 text-sm" [disabled]="!selectedTypeCode" (click)="requestCertificate()">
              Solicitar certificado
            </button>
          </div>
        </article>

        <aside class="rounded-xl border border-slate-200 bg-white p-4 lg:col-span-5">
          <p class="text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Flujo recomendado</p>
          <ol class="mt-3 space-y-2 text-sm text-slate-700">
            <li class="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2"><span class="font-semibold text-[color:var(--umg-navy-900)]">1.</span> Selecciona tipo de certificacion.</li>
            <li class="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2"><span class="font-semibold text-[color:var(--umg-navy-900)]">2.</span> Genera la solicitud de tramite.</li>
            <li class="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2"><span class="font-semibold text-[color:var(--umg-navy-900)]">3.</span> Completa pago si esta pendiente.</li>
            <li class="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2"><span class="font-semibold text-[color:var(--umg-navy-900)]">4.</span> Revisa estado y descarga cuando este disponible.</li>
          </ol>

          <div class="mt-4 flex gap-2">
            <span class="rounded-full border border-amber-200 bg-amber-50 px-3 py-1 text-xs font-semibold text-amber-700">Pago pendiente: {{ pendingCertificateCount }}</span>
            <span class="rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">Pagadas: {{ paidCertificateCount }}</span>
          </div>
        </aside>
      </div>

      <section *ngIf="checkoutCertificate" class="mt-6 rounded-xl border border-slate-200 bg-slate-50 p-4 sm:p-5">
        <header class="mb-4 flex flex-wrap items-start justify-between gap-3">
          <div>
            <p class="text-xs font-semibold uppercase tracking-[0.1em] text-[color:var(--umg-navy-700)]">Checkout demo</p>
            <h3 class="font-display mt-1 text-xl font-bold text-[color:var(--umg-navy-900)]">Pago de certificacion</h3>
            <p class="mt-1 text-sm text-muted">Estas pagando la solicitud: {{ checkoutCertificate.purpose }}</p>
          </div>
          <div class="rounded-lg border border-slate-200 bg-white px-3 py-2 text-right">
            <p class="text-[11px] font-semibold uppercase tracking-[0.08em] text-slate-500">Monto</p>
            <p class="mt-1 font-display text-xl font-bold text-[color:var(--umg-navy-900)]">Q{{ checkoutCertificate.amount | number:'1.2-2' }}</p>
            <p class="text-xs text-slate-500">{{ checkoutCertificate.currency }}</p>
          </div>
        </header>

        <div class="grid gap-4 lg:grid-cols-12">
          <article class="rounded-xl border border-slate-200 bg-white p-4 lg:col-span-4">
            <p class="text-xs font-semibold uppercase tracking-[0.08em] text-slate-500">Resumen de orden</p>
            <p class="mt-2 text-sm"><span class="font-semibold text-slate-700">Tipo:</span> {{ checkoutCertificate.purpose }}</p>
            <p class="mt-1 text-sm"><span class="font-semibold text-slate-700">Estado:</span> {{ checkoutCertificate.status }}</p>
            <p class="mt-1 text-sm"><span class="font-semibold text-slate-700">Pago:</span> {{ checkoutCertificate.paymentStatus }}</p>
            <p class="mt-2 text-xs text-muted">Entorno de demostracion. No se almacenan datos sensibles completos.</p>
          </article>

          <article class="rounded-xl border border-slate-200 bg-white p-4 lg:col-span-8">
            <div class="grid gap-3 md:grid-cols-2">
              <div class="md:col-span-2">
                <label class="mb-1 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Nombre del titular</label>
                <input class="input-control" [(ngModel)]="checkout.cardHolderName" placeholder="Nombre completo" />
              </div>

              <div class="md:col-span-2">
                <label class="mb-1 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Numero de tarjeta</label>
                <input class="input-control" [(ngModel)]="checkout.cardNumber" placeholder="0000 0000 0000 0000" />
              </div>

              <div>
                <label class="mb-1 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Mes</label>
                <input class="input-control" type="number" [(ngModel)]="checkout.expiryMonth" placeholder="MM" />
              </div>

              <div>
                <label class="mb-1 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Anio</label>
                <input class="input-control" type="number" [(ngModel)]="checkout.expiryYear" placeholder="YYYY" />
              </div>

              <div class="md:col-span-2">
                <label class="mb-1 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">CVV</label>
                <input class="input-control" [(ngModel)]="checkout.cvv" placeholder="***" />
              </div>
            </div>

            <div class="mt-4 flex flex-wrap gap-2">
              <button class="btn-primary px-4 py-2 text-sm" [disabled]="checkoutLoading" (click)="submitCheckout()">
                {{ checkoutLoading ? 'Procesando...' : 'Confirmar pago' }}
              </button>
              <button class="btn-secondary px-4 py-2 text-sm" [disabled]="checkoutLoading" (click)="cancelCheckout()">
                Cancelar
              </button>
            </div>
          </article>
        </div>
      </section>

      <div class="mt-4 space-y-2">
        <p *ngIf="message" class="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{{ message }}</p>
        <p *ngIf="error" class="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">{{ error }}</p>
      </div>
    </section>

    <section class="panel mt-6 p-6">
      <div class="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h3 class="section-title text-lg">Mis certificaciones</h3>
          <p class="mt-1 text-sm text-muted">Historial de tramites y acciones disponibles por estado.</p>
        </div>
        <span class="rounded-full border border-slate-200 bg-slate-50 px-3 py-1 text-xs font-semibold text-slate-600">{{ certificates.length }} registro(s)</span>
      </div>

      <div *ngIf="certificates.length === 0" class="rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-10 text-center">
        <p class="font-display text-lg font-bold text-[color:var(--umg-navy-900)]">Aun no tienes certificaciones</p>
        <p class="mt-1 text-sm text-muted">Solicita un certificado para ver su estado y gestionar acciones.</p>
      </div>

      <div *ngIf="certificates.length > 0" class="space-y-4">
        <div class="hidden overflow-x-auto rounded-xl border border-slate-200 md:block">
          <table class="table-clean w-full text-left text-sm">
            <thead class="bg-slate-50">
              <tr class="border-b border-slate-200">
                <th class="py-3 pl-4">Fecha</th>
                <th class="py-3">Motivo</th>
                <th class="py-3">Monto</th>
                <th class="py-3">Pago</th>
                <th class="py-3">Estado</th>
                <th class="py-3 pr-4 text-right">Accion</th>
              </tr>
            </thead>
            <tbody>
              <tr
                *ngFor="let item of certificates"
                class="border-b border-slate-100 transition hover:bg-slate-50/70"
                [class.bg-sky-50]="checkoutCertificate?.id === item.id"
              >
                <td class="py-3 pl-4">{{ item.createdAt | date:'short' }}</td>
                <td class="py-3 max-w-[20rem] font-medium text-slate-700">{{ item.purpose }}</td>
                <td class="py-3 font-semibold text-[color:var(--umg-navy-900)]">Q{{ item.amount | number:'1.2-2' }} {{ item.currency }}</td>
                <td class="py-3"><app-status-badge [label]="item.paymentStatus"></app-status-badge></td>
                <td class="py-3"><app-status-badge [label]="item.status"></app-status-badge></td>
                <td class="py-3 pr-4 text-right">
                  <div class="flex justify-end gap-2">
                    <button
                      *ngIf="item.status === 'Requested' && item.paymentStatus === 'Pending'"
                      class="btn-primary px-3 py-1.5 text-xs"
                      (click)="openCheckout(item)"
                    >
                      Pagar
                    </button>
                    <button
                      *ngIf="item.status === 'Requested' && item.paymentStatus === 'Paid'"
                      class="btn-secondary px-3 py-1.5 text-xs"
                      (click)="generateCertificate(item.id)"
                    >
                      Generar
                    </button>
                    <button
                      *ngIf="item.pdfAvailable"
                      class="btn-secondary px-3 py-1.5 text-xs"
                      (click)="downloadCertificate(item.id)"
                    >
                      Ver/Descargar
                    </button>
                    <button
                      *ngIf="item.status === 'Requested' && item.paymentStatus === 'Pending'"
                      class="btn-danger px-3 py-1.5 text-xs"
                      (click)="cancelCertificate(item.id)"
                    >
                      Cancelar
                    </button>
                    <span
                      *ngIf="
                        !(item.status === 'Requested' && item.paymentStatus === 'Pending') &&
                        !(item.status === 'Requested' && item.paymentStatus === 'Paid') &&
                        !item.pdfAvailable
                      "
                      class="text-xs text-slate-400"
                    >
                      Sin accion
                    </span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="space-y-3 md:hidden">
          <article
            *ngFor="let item of certificates"
            class="rounded-xl border border-slate-200 bg-white p-4"
            [ngClass]="{ 'border-[color:var(--umg-navy-700)] bg-sky-50': checkoutCertificate?.id === item.id }"
          >
            <div class="flex items-start justify-between gap-3">
              <div>
                <p class="font-display font-bold text-[color:var(--umg-navy-900)]">{{ item.purpose }}</p>
                <p class="text-xs text-muted">{{ item.createdAt | date:'short' }}</p>
              </div>
              <app-status-badge [label]="item.status"></app-status-badge>
            </div>
            <p class="mt-2 text-sm font-semibold text-[color:var(--umg-navy-900)]">Q{{ item.amount | number:'1.2-2' }} {{ item.currency }}</p>
            <div class="mt-2"><app-status-badge [label]="item.paymentStatus"></app-status-badge></div>
            <div class="mt-3 flex flex-wrap gap-2">
              <button
                *ngIf="item.status === 'Requested' && item.paymentStatus === 'Pending'"
                class="btn-primary px-3 py-1.5 text-xs"
                (click)="openCheckout(item)"
              >
                Pagar
              </button>
              <button
                *ngIf="item.status === 'Requested' && item.paymentStatus === 'Paid'"
                class="btn-secondary px-3 py-1.5 text-xs"
                (click)="generateCertificate(item.id)"
              >
                Generar
              </button>
              <button
                *ngIf="item.pdfAvailable"
                class="btn-secondary px-3 py-1.5 text-xs"
                (click)="downloadCertificate(item.id)"
              >
                Ver/Descargar
              </button>
              <button
                *ngIf="item.status === 'Requested' && item.paymentStatus === 'Pending'"
                class="btn-danger px-3 py-1.5 text-xs"
                (click)="cancelCertificate(item.id)"
              >
                Cancelar
              </button>
            </div>
          </article>
        </div>
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

  get selectedType(): CertificateTypeResponse | undefined {
    return this.certificateTypes.find((type) => type.code === this.selectedTypeCode);
  }

  get pendingCertificateCount(): number {
    return this.certificates.filter((x) => x.paymentStatus === 'Pending').length;
  }

  get paidCertificateCount(): number {
    return this.certificates.filter((x) => x.paymentStatus === 'Paid').length;
  }

  constructor() {
    this.loadCertificateTypes();
    this.loadMyCertificates();
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
      this.error = 'Debes seleccionar un tipo de certificacion.';
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

        this.message = 'Solicitud de certificacion cancelada.';
        this.loadMyCertificates();
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

    return 'Error de conexion.';
  }
}
