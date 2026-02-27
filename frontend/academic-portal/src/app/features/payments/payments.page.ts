import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { API_BASE_URL } from '../../core/config/api.config';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'app-payments-page',
  imports: [CommonModule, FormsModule, StatusBadgeComponent],
  template: `
    <section class="rounded-2xl bg-white p-6 shadow-sm">
      <h2 class="text-lg font-bold">Mis ordenes de pago</h2>
      <div class="mt-4 overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead>
            <tr class="border-b text-slate-500">
              <th class="py-2">Fecha</th>
              <th>Tipo</th>
              <th>Monto</th>
              <th>Descripcion</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let payment of payments" class="border-b">
              <td class="py-2">{{ payment.createdAt | date:'short' }}</td>
              <td>{{ payment.orderType }}</td>
              <td>{{ payment.amount | currency:'USD':'symbol':'1.0-0' }}</td>
              <td>{{ payment.description }}</td>
              <td><app-status-badge [label]="payment.status"></app-status-badge></td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="mt-6 rounded-2xl bg-white p-6 shadow-sm" *ngIf="auth.me()?.role === 'Admin'">
      <h3 class="text-lg font-bold">Marcar pago como completado (demo)</h3>
      <div class="mt-3 flex gap-2">
        <input
          [(ngModel)]="paymentId"
          placeholder="Payment ID"
          class="w-full rounded-lg border border-slate-300 px-3 py-2"
        />
        <button class="rounded-lg bg-emerald-700 px-4 py-2 text-white" (click)="markPaid()">Marcar</button>
      </div>
      <p *ngIf="message" class="mt-2 text-sm text-emerald-700">{{ message }}</p>
      <p *ngIf="error" class="mt-2 text-sm text-rose-700">{{ error }}</p>
    </section>
  `
})
export class PaymentsPage {
  readonly auth = inject(AuthService);
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  payments: any[] = [];
  paymentId = '';
  message = '';
  error = '';

  constructor() {
    this.loadPayments();
  }

  loadPayments(): void {
    this.http.get<any>(`${this.baseUrl}/payments/my`).subscribe((response) => {
      this.payments = response.success ? response.data : [];
    });
  }

  markPaid(): void {
    this.message = '';
    this.error = '';
    this.http.post<any>(`${this.baseUrl}/payments/${this.paymentId}/mark-paid`, {}).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No se pudo actualizar el pago.';
          return;
        }

        this.message = `Pago ${response.data.id} marcado como ${response.data.status}`;
        this.loadPayments();
      },
      error: () => {
        this.error = 'Error de conexion.';
      }
    });
  }
}
