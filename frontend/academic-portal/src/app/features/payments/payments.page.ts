import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { API_BASE_URL } from '../../core/config/api.config';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { ApiEnvelope, PaymentOrderResponse } from '../../shared/models/api.models';

@Component({
  standalone: true,
  selector: 'app-payments-page',
  imports: [CommonModule, StatusBadgeComponent],
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
            </tr>
          </tbody>
        </table>
      </div>
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
        this.error = error.error?.error?.message ?? 'Error de conexión.';
      }
    });
  }
}
