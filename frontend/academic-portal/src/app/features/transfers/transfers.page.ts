import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import {
  ApiEnvelope,
  CampusResponse,
  TransferAvailabilityResponse,
  TransferResponse
} from '../../shared/models/api.models';

@Component({
  standalone: true,
  selector: 'app-transfers-page',
  imports: [CommonModule, FormsModule, StatusBadgeComponent],
  template: `
    <section class="panel p-6">
      <h2 class="section-title text-xl">Solicitud de Traslado de Sede</h2>
      <p class="mt-1 text-sm text-muted">Selecciona campus/centro y jornada para validar cupo.</p>

      <div class="mt-4 grid gap-3 md:grid-cols-3">
        <select class="input-control" [(ngModel)]="campusId">
          <option [ngValue]="0">Selecciona sede</option>
          <option *ngFor="let campus of campuses" [ngValue]="campus.id">
            {{ campus.name }} · {{ campus.campusType }}{{ campus.region ? ' · ' + campus.region : '' }}
          </option>
        </select>

        <select class="input-control" [(ngModel)]="shift">
          <option value="Saturday">Sábado</option>
          <option value="Sunday">Domingo</option>
        </select>

        <button class="btn-secondary px-4 py-2" (click)="checkAvailability()">Validar cupo</button>
      </div>

      <div class="mt-3 grid gap-3 md:grid-cols-2">
        <select class="input-control" [(ngModel)]="modality">
          <option value="Presencial">Presencial</option>
          <option value="Virtual">Virtual</option>
        </select>
      </div>

      <textarea
        class="input-control mt-3"
        rows="3"
        [(ngModel)]="reason"
        placeholder="Motivo del traslado"
      ></textarea>

      <p *ngIf="availabilityText" class="mt-2 text-sm text-[color:var(--umg-navy-900)]">{{ availabilityText }}</p>
      <p *ngIf="message" class="mt-2 text-sm text-emerald-700">{{ message }}</p>
      <p *ngIf="error" class="mt-2 text-sm text-rose-700">{{ error }}</p>

      <button class="btn-primary mt-4 px-4 py-2" (click)="createTransfer()" [disabled]="campusId <= 0">
        Crear solicitud
      </button>
    </section>

    <section class="panel mt-6 p-6">
      <h3 class="section-title text-lg">Mis solicitudes</h3>
      <div class="mt-4 overflow-x-auto">
        <table class="table-clean w-full text-left text-sm">
          <thead>
            <tr class="border-b border-slate-200">
              <th class="py-2">Fecha</th>
              <th>Origen</th>
              <th>Destino</th>
              <th>Jornada</th>
              <th>Modalidad</th>
              <th>Estado</th>
              <th class="text-right">Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of transfers" class="border-b border-slate-100">
              <td class="py-2">{{ item.createdAt | date:'short' }}</td>
              <td>{{ item.fromCampus }}</td>
              <td>{{ item.toCampus }}</td>
              <td>{{ item.shift === 'Saturday' ? 'Sábado' : 'Domingo' }}</td>
              <td>{{ item.modality }}</td>
              <td><app-status-badge [label]="item.status"></app-status-badge></td>
              <td class="text-right">
                <button
                  *ngIf="item.status === 'PendingPayment'"
                  class="btn-danger px-3 py-1 text-xs"
                  (click)="cancelTransfer(item.transferId)"
                >
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
export class TransfersPage {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  campuses: CampusResponse[] = [];
  transfers: TransferResponse[] = [];
  campusId = 0;
  shift: 'Saturday' | 'Sunday' = 'Saturday';
  modality: 'Presencial' | 'Virtual' = 'Presencial';
  reason = '';
  availabilityText = '';
  message = '';
  error = '';

  constructor() {
    this.loadCampuses();
    this.loadMyTransfers();
  }

  loadCampuses(): void {
    this.http.get<ApiEnvelope<CampusResponse[]>>(`${this.baseUrl}/campuses`).subscribe((response) => {
      if (response.success) {
        this.campuses = response.data;
      }
    });
  }

  loadMyTransfers(): void {
    this.http.get<ApiEnvelope<TransferResponse[]>>(`${this.baseUrl}/transfers/my`).subscribe((response) => {
      this.transfers = response.success ? response.data : [];
    });
  }

  checkAvailability(): void {
    this.error = '';
    this.http
      .get<ApiEnvelope<TransferAvailabilityResponse>>(`${this.baseUrl}/transfers/availability`, {
        params: { campusId: this.campusId, shift: this.shift }
      })
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo validar disponibilidad.';
            return;
          }

          const data = response.data;
          this.availabilityText = `Cupos: ${data.availableCapacity}/${data.totalCapacity} en ${data.campusName} - ${data.shiftName === 'Saturday' ? 'Sábado' : 'Domingo'}`;
        },
        error: (error: HttpErrorResponse) => {
          this.error = error.error?.error?.message ?? 'Error de conexión.';
        }
      });
  }

  createTransfer(): void {
    this.error = '';
    this.message = '';

    this.http
      .post<ApiEnvelope<{ paymentOrderId: string; amount: number; currency: string; expiresAt: string }>>(`${this.baseUrl}/transfers`, {
        campusId: this.campusId,
        shift: this.shift,
        modality: this.modality,
        reason: this.reason
      })
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo crear la solicitud.';
            return;
          }

          this.message = `Solicitud creada (${this.modality}). Orden de pago ${response.data.paymentOrderId} por Q${response.data.amount.toFixed(2)} (${response.data.currency}).`;
          this.loadMyTransfers();
        },
        error: (error: HttpErrorResponse) => {
          this.error = error.error?.error?.message ?? 'Error de conexión.';
        }
      });
  }

  cancelTransfer(transferId: string): void {
    this.error = '';
    this.message = '';

    this.http.post<ApiEnvelope<{ transferId: string }>>(`${this.baseUrl}/transfers/${transferId}/cancel`, {}).subscribe({
      next: (response) => {
        if (!response.success) {
          this.error = response.error?.message ?? 'No se pudo cancelar la solicitud.';
          return;
        }

        this.message = 'Solicitud de traslado cancelada.';
        this.loadMyTransfers();
      },
      error: (error: HttpErrorResponse) => {
        this.error = error.error?.error?.message ?? 'Error de conexión.';
      }
    });
  }
}
