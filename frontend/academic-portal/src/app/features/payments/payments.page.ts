import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { API_BASE_URL } from '../../core/config/api.config';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { ApiEnvelope, MockCheckoutRequest, MockCheckoutResponse, PaymentOrderResponse } from '../../shared/models/api.models';

@Component({
  standalone: true,
  selector: 'app-payments-page',
  imports: [CommonModule, FormsModule, StatusBadgeComponent],
  template: `
    <section class="panel p-6" *ngIf="auth.me()?.role === 'Student'">
      <header class="mb-5 flex flex-wrap items-end justify-between gap-3">
        <div>
          <p class="text-xs font-semibold uppercase tracking-[0.12em] text-[color:var(--umg-navy-700)]">Modulo financiero estudiantil</p>
          <h2 class="section-title mt-1 text-2xl">Mis ordenes de pago</h2>
          <p class="mt-1 text-sm text-muted">Revisa tus pagos pendientes y completa checkout de forma guiada.</p>
        </div>
        <div class="flex gap-2">
          <span class="rounded-full border border-amber-200 bg-amber-50 px-3 py-1 text-xs font-semibold text-amber-700">
            Pendientes: {{ pendingStudentCount }}
          </span>
          <span class="rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
            Pagadas: {{ paidStudentCount }}
          </span>
        </div>
      </header>

      <div *ngIf="payments.length === 0" class="rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-10 text-center">
        <p class="font-display text-lg font-bold text-[color:var(--umg-navy-900)]">No hay ordenes registradas</p>
        <p class="mt-1 text-sm text-muted">Cuando el sistema genere una orden, aparecera aqui con su estado y accion.</p>
      </div>

      <div *ngIf="payments.length > 0" class="space-y-4">
        <div class="hidden overflow-x-auto rounded-xl border border-slate-200 md:block">
          <table class="table-clean w-full text-left text-sm">
            <thead class="bg-slate-50">
              <tr class="border-b border-slate-200">
                <th class="py-3 pl-4">Fecha</th>
                <th class="py-3">Tipo</th>
                <th class="py-3">Monto</th>
                <th class="py-3">Vence</th>
                <th class="py-3">Descripcion</th>
                <th class="py-3">Estado</th>
                <th class="py-3 pr-4 text-right">Accion</th>
              </tr>
            </thead>
            <tbody>
              <tr
                *ngFor="let payment of payments"
                class="border-b border-slate-100 transition hover:bg-slate-50/70"
                [class.bg-sky-50]="isCheckoutSelected(payment)"
              >
                <td class="py-3 pl-4">{{ payment.createdAt | date:'short' }}</td>
                <td class="py-3">{{ payment.orderType }}</td>
                <td class="py-3 font-semibold text-[color:var(--umg-navy-900)]">Q{{ payment.amount | number:'1.2-2' }} {{ payment.currency }}</td>
                <td class="py-3">{{ payment.expiresAt | date:'short' }}</td>
                <td class="py-3 max-w-[22rem] text-slate-700">{{ payment.description }}</td>
                <td class="py-3"><app-status-badge [label]="payment.status"></app-status-badge></td>
                <td class="py-3 pr-4 text-right">
                  <button
                    *ngIf="payment.status === 'Pending'"
                    class="btn-primary px-3 py-1.5 text-xs"
                    (click)="openCheckout(payment)"
                  >
                    Pagar con tarjeta demo
                  </button>
                  <span *ngIf="payment.status !== 'Pending'" class="text-xs text-slate-400">Sin accion</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="space-y-3 md:hidden">
          <article
            *ngFor="let payment of payments"
            class="rounded-xl border border-slate-200 bg-white p-4"
            [ngClass]="{ 'border-[color:var(--umg-navy-700)] bg-sky-50': isCheckoutSelected(payment) }"
          >
            <div class="flex items-start justify-between gap-3">
              <div>
                <p class="font-display font-bold text-[color:var(--umg-navy-900)]">{{ payment.orderType }}</p>
                <p class="text-xs text-muted">{{ payment.createdAt | date:'short' }}</p>
              </div>
              <app-status-badge [label]="payment.status"></app-status-badge>
            </div>
            <p class="mt-2 text-sm font-semibold text-[color:var(--umg-navy-900)]">Q{{ payment.amount | number:'1.2-2' }} {{ payment.currency }}</p>
            <p class="mt-1 text-xs text-slate-600">Vence: {{ payment.expiresAt | date:'short' }}</p>
            <p class="mt-2 text-sm text-slate-700">{{ payment.description }}</p>
            <div class="mt-3">
              <button
                *ngIf="payment.status === 'Pending'"
                class="btn-primary w-full px-3 py-2 text-xs"
                (click)="openCheckout(payment)"
              >
                Pagar con tarjeta demo
              </button>
            </div>
          </article>
        </div>
      </div>

      <section *ngIf="checkoutPayment" id="checkout-card" class="mt-6 rounded-xl border border-slate-200 bg-slate-50 p-4 sm:p-5">
        <header class="mb-4 flex flex-wrap items-start justify-between gap-3">
          <div>
            <p class="text-xs font-semibold uppercase tracking-[0.1em] text-[color:var(--umg-navy-700)]">Checkout demo</p>
            <h3 class="font-display mt-1 text-xl font-bold text-[color:var(--umg-navy-900)]">Confirmacion de pago</h3>
            <p class="mt-1 text-sm text-muted">Estas pagando una orden {{ checkoutPayment.orderType }} en entorno de demostracion.</p>
          </div>
          <div class="rounded-lg border border-slate-200 bg-white px-3 py-2 text-right">
            <p class="text-[11px] font-semibold uppercase tracking-[0.08em] text-slate-500">Monto a pagar</p>
            <p class="mt-1 font-display text-xl font-bold text-[color:var(--umg-navy-900)]">Q{{ checkoutPayment.amount | number:'1.2-2' }}</p>
            <p class="text-xs text-slate-500">{{ checkoutPayment.currency }}</p>
          </div>
        </header>

        <div class="grid gap-4 lg:grid-cols-12">
          <article class="rounded-xl border border-slate-200 bg-white p-4 lg:col-span-4">
            <p class="text-xs font-semibold uppercase tracking-[0.08em] text-slate-500">Detalle de orden</p>
            <p class="mt-2 text-sm"><span class="font-semibold text-slate-700">Tipo:</span> {{ checkoutPayment.orderType }}</p>
            <p class="mt-1 text-sm"><span class="font-semibold text-slate-700">Estado:</span> {{ checkoutPayment.status }}</p>
            <p class="mt-1 text-sm"><span class="font-semibold text-slate-700">Vence:</span> {{ checkoutPayment.expiresAt | date:'short' }}</p>
            <p class="mt-2 text-xs text-muted">La informacion de tarjeta se usa solo para simulacion.</p>
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
              <button class="btn-primary px-4 py-2 text-sm" [disabled]="checkoutLoading" (click)="submitMockCheckout()">
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

    <section class="panel mt-6 p-6" *ngIf="auth.me()?.role === 'Admin'">
      <h3 class="section-title text-lg">Pagos pendientes (administracion)</h3>
      <div class="mt-4 overflow-x-auto rounded-xl border border-slate-200">
        <table class="table-clean w-full text-left text-sm">
          <thead class="bg-slate-50">
            <tr class="border-b border-slate-200">
              <th class="py-3 pl-4">ID Pago</th>
              <th class="py-3">Tipo</th>
              <th class="py-3">Monto</th>
              <th class="py-3">Vence</th>
              <th class="py-3 pr-4 text-right">Accion</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let payment of pendingPayments" class="border-b border-slate-100 transition hover:bg-slate-50/70">
              <td class="py-3 pl-4">{{ payment.id }}</td>
              <td class="py-3">{{ payment.orderType }}</td>
              <td class="py-3 font-semibold text-[color:var(--umg-navy-900)]">Q{{ payment.amount | number:'1.2-2' }} {{ payment.currency }}</td>
              <td class="py-3">{{ payment.expiresAt | date:'short' }}</td>
              <td class="py-3 pr-4 text-right">
                <button class="btn-primary px-3 py-1.5 text-xs" (click)="markPaid(payment.id)">Marcar pagado</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="mt-4 space-y-2">
        <p *ngIf="message" class="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{{ message }}</p>
        <p *ngIf="error" class="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">{{ error }}</p>
      </div>
    </section>
  `
})
export class PaymentsPage {
  readonly auth = inject(AuthService);
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  payments: PaymentOrderResponse[] = [];
  pendingPayments: PaymentOrderResponse[] = [];
  checkoutPayment: PaymentOrderResponse | null = null;
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

  get pendingStudentCount(): number {
    return this.payments.filter((x) => x.status === 'Pending').length;
  }

  get paidStudentCount(): number {
    return this.payments.filter((x) => x.status === 'Paid').length;
  }

  constructor() {
    if (this.auth.me()?.role === 'Student') {
      this.loadPayments();
    }

    if (this.auth.me()?.role === 'Admin') {
      this.loadPendingPayments();
    }
  }

  loadPayments(): void {
    this.http.get<ApiEnvelope<PaymentOrderResponse[]>>(`${this.baseUrl}/payments/my`).subscribe((response) => {
      this.payments = response.success ? response.data : [];
    });
  }

  loadPendingPayments(): void {
    this.http.get<ApiEnvelope<PaymentOrderResponse[]>>(`${this.baseUrl}/payments/pending`).subscribe((response) => {
      this.pendingPayments = response.success ? response.data : [];
    });
  }

  markPaid(paymentId: string): void {
    this.message = '';
    this.error = '';

    this.http.post<ApiEnvelope<PaymentOrderResponse>>(`${this.baseUrl}/payments/${paymentId}/mark-paid`, {}).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No se pudo actualizar el pago.';
          return;
        }

        this.message = `Pago ${response.data.id} marcado como ${response.data.status}.`;
        if (this.auth.me()?.role === 'Student') {
          this.loadPayments();
        }
        this.loadPendingPayments();
      },
      error: (error: HttpErrorResponse) => {
        this.error = this.extractErrorMessage(error);
      }
    });
  }

  openCheckout(payment: PaymentOrderResponse): void {
    this.error = '';
    this.message = '';
    this.checkoutPayment = payment;
  }

  cancelCheckout(): void {
    this.checkoutPayment = null;
    this.checkoutLoading = false;
  }

  isCheckoutSelected(payment: PaymentOrderResponse): boolean {
    return this.checkoutPayment?.id === payment.id;
  }

  submitMockCheckout(): void {
    if (!this.checkoutPayment || this.checkoutLoading) {
      return;
    }

    this.error = '';
    this.message = '';
    this.checkoutLoading = true;

    this.http
      .post<ApiEnvelope<MockCheckoutResponse>>(`${this.baseUrl}/payments/${this.checkoutPayment.id}/mock-checkout`, this.checkout)
      .subscribe({
        next: (response) => {
          this.checkoutLoading = false;
          if (!response.success) {
            this.error = response.error?.message ?? 'No fue posible completar el pago.';
            return;
          }

          this.message = `Pago aprobado: ${response.data.payment.orderType} (${response.data.payment.status}).`;
          const generatedCertificate = response.data.certificate;
          if (generatedCertificate?.pdfAvailable) {
            this.downloadCertificate(generatedCertificate.certificateId);
            this.message += ` Certificado ${generatedCertificate.certificateId} generado y listo para descarga.`;
          }

          const enrollmentDire = response.data.enrollmentDire;
          if (enrollmentDire?.pdfAvailable) {
            this.downloadEnrollmentDire(enrollmentDire.enrollmentId);
            this.message += ` DIRE de inscripcion ${enrollmentDire.direNumber} generado y descargado.`;
          }

          this.checkoutPayment = null;
          this.loadPayments();
          if (this.auth.me()?.role === 'Admin') {
            this.loadPendingPayments();
          }
        },
        error: (error: HttpErrorResponse) => {
          this.checkoutLoading = false;
          this.error = this.extractErrorMessage(error);
        }
      });
  }

  private downloadCertificate(certificateId: string): void {
    this.http.get(`${this.baseUrl}/certificates/${certificateId}/download`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `certificate-${certificateId}.pdf`;
        anchor.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }

  private downloadEnrollmentDire(enrollmentId: string): void {
    this.http.get(`${this.baseUrl}/enrollments/${enrollmentId}/dire`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `dire-inscripcion-${enrollmentId}.pdf`;
        anchor.click();
        window.URL.revokeObjectURL(url);
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
