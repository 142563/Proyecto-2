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
      <h2 class="section-title text-lg">Mis órdenes de pago</h2>
      <div class="mt-4 overflow-x-auto">
        <table class="table-clean w-full text-left text-sm">
          <thead>
            <tr class="border-b border-slate-200">
              <th class="py-2">Fecha</th>
              <th>Tipo</th>
              <th>Monto</th>
              <th>Vence</th>
              <th>Descripción</th>
              <th>Estado</th>
              <th class="text-right">Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let payment of payments" class="border-b border-slate-100">
              <td class="py-2">{{ payment.createdAt | date:'short' }}</td>
              <td>{{ payment.orderType }}</td>
              <td>Q{{ payment.amount | number:'1.2-2' }} {{ payment.currency }}</td>
              <td>{{ payment.expiresAt | date:'short' }}</td>
              <td>{{ payment.description }}</td>
              <td><app-status-badge [label]="payment.status"></app-status-badge></td>
              <td class="text-right">
                <button
                  *ngIf="payment.status === 'Pending'"
                  class="btn-primary px-3 py-1 text-xs"
                  (click)="openCheckout(payment)"
                >
                  Pagar con tarjeta (demo)
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div *ngIf="checkoutPayment" class="mt-5 rounded-lg border border-slate-200 bg-slate-50 p-4">
        <h3 class="text-sm font-semibold uppercase tracking-wide text-[color:var(--umg-navy-700)]">Checkout demo</h3>
        <p class="mt-1 text-sm text-muted">
          Pago: {{ checkoutPayment.orderType }} · Q{{ checkoutPayment.amount | number:'1.2-2' }} {{ checkoutPayment.currency }}
        </p>

        <div class="mt-3 grid gap-3 md:grid-cols-2">
          <input class="input-control" [(ngModel)]="checkout.cardHolderName" placeholder="Nombre del titular" />
          <input class="input-control" [(ngModel)]="checkout.cardNumber" placeholder="Número de tarjeta" />
          <input class="input-control" type="number" [(ngModel)]="checkout.expiryMonth" placeholder="Mes (MM)" />
          <input class="input-control" type="number" [(ngModel)]="checkout.expiryYear" placeholder="Año (YYYY)" />
          <input class="input-control md:col-span-2" [(ngModel)]="checkout.cvv" placeholder="CVV" />
        </div>

        <div class="mt-3 flex gap-2">
          <button class="btn-primary px-4 py-2 text-sm" [disabled]="checkoutLoading" (click)="submitMockCheckout()">
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

    <section class="panel mt-6 p-6" *ngIf="auth.me()?.role === 'Admin'">
      <h3 class="section-title text-lg">Pagos pendientes (administración)</h3>
      <div class="mt-4 overflow-x-auto">
        <table class="table-clean w-full text-left text-sm">
          <thead>
            <tr class="border-b border-slate-200">
              <th class="py-2">ID Pago</th>
              <th>Tipo</th>
              <th>Monto</th>
              <th>Vence</th>
              <th class="text-right">Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let payment of pendingPayments" class="border-b border-slate-100">
              <td class="py-2">{{ payment.id }}</td>
              <td>{{ payment.orderType }}</td>
              <td>Q{{ payment.amount | number:'1.2-2' }} {{ payment.currency }}</td>
              <td>{{ payment.expiresAt | date:'short' }}</td>
              <td class="text-right">
                <button class="btn-primary px-3 py-1 text-xs" (click)="markPaid(payment.id)">Marcar pagado</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <p *ngIf="message" class="mt-3 text-sm text-emerald-700">{{ message }}</p>
      <p *ngIf="error" class="mt-2 text-sm text-rose-700">{{ error }}</p>
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
            this.message += ` DIRE de inscripción ${enrollmentDire.direNumber} generado y descargado.`;
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

    return 'Error de conexión.';
  }
}
