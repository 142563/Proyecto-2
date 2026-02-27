import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'app-transfers-page',
  imports: [CommonModule, FormsModule, StatusBadgeComponent],
  template: `
    <section class="rounded-2xl bg-white p-6 shadow-sm">
      <h2 class="text-lg font-bold">Solicitud de Traslado</h2>
      <div class="mt-4 grid gap-3 md:grid-cols-3">
        <select class="rounded-lg border border-slate-300 px-3 py-2" [(ngModel)]="campusId">
          <option [ngValue]="0">Selecciona sede</option>
          <option *ngFor="let campus of campuses" [ngValue]="campus.id">{{ campus.name }}</option>
        </select>

        <select class="rounded-lg border border-slate-300 px-3 py-2" [(ngModel)]="shift">
          <option value="Saturday">Saturday</option>
          <option value="Sunday">Sunday</option>
        </select>

        <button class="rounded-lg bg-slate-900 px-4 py-2 text-white" (click)="checkAvailability()">Validar cupo</button>
      </div>

      <textarea
        class="mt-3 w-full rounded-lg border border-slate-300 px-3 py-2"
        rows="3"
        [(ngModel)]="reason"
        placeholder="Motivo del traslado"
      ></textarea>

      <p *ngIf="availabilityText" class="mt-2 text-sm text-slate-600">{{ availabilityText }}</p>
      <p *ngIf="message" class="mt-2 text-sm text-emerald-700">{{ message }}</p>
      <p *ngIf="error" class="mt-2 text-sm text-rose-700">{{ error }}</p>

      <button class="mt-4 rounded-lg bg-emerald-700 px-4 py-2 text-white" (click)="createTransfer()">
        Crear solicitud
      </button>
    </section>

    <section class="mt-6 rounded-2xl bg-white p-6 shadow-sm">
      <h3 class="text-lg font-bold">Mis solicitudes</h3>
      <div class="mt-4 overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead>
            <tr class="border-b text-slate-500">
              <th class="py-2">Fecha</th>
              <th>Origen</th>
              <th>Destino</th>
              <th>Jornada</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of transfers" class="border-b">
              <td class="py-2">{{ item.createdAt | date:'short' }}</td>
              <td>{{ item.fromCampus }}</td>
              <td>{{ item.toCampus }}</td>
              <td>{{ item.shift }}</td>
              <td><app-status-badge [label]="item.status"></app-status-badge></td>
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

  campuses: any[] = [];
  transfers: any[] = [];
  campusId = 0;
  shift = 'Saturday';
  reason = '';
  availabilityText = '';
  message = '';
  error = '';

  constructor() {
    this.loadCampuses();
    this.loadMyTransfers();
  }

  loadCampuses(): void {
    this.http.get<any>(`${this.baseUrl}/campuses`).subscribe((response) => {
      if (response.success) {
        this.campuses = response.data;
      }
    });
  }

  loadMyTransfers(): void {
    this.http.get<any>(`${this.baseUrl}/transfers/my`).subscribe((response) => {
      this.transfers = response.success ? response.data : [];
    });
  }

  checkAvailability(): void {
    this.error = '';
    this.http
      .get<any>(`${this.baseUrl}/transfers/availability`, {
        params: { campusId: this.campusId, shift: this.shift }
      })
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo validar disponibilidad.';
            return;
          }

          const data = response.data;
          this.availabilityText = `Cupos: ${data.availableCapacity}/${data.totalCapacity} en ${data.campusName} - ${data.shiftName}`;
        },
        error: () => {
          this.error = 'Error de conexion.';
        }
      });
  }

  createTransfer(): void {
    this.error = '';
    this.message = '';

    this.http
      .post<any>(`${this.baseUrl}/transfers`, {
        campusId: this.campusId,
        shift: this.shift,
        reason: this.reason
      })
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo crear la solicitud.';
            return;
          }

          this.message = `Solicitud creada. Orden de pago: ${response.data.paymentOrderId}`;
          this.loadMyTransfers();
        },
        error: () => {
          this.error = 'Error de conexion.';
        }
      });
  }
}
