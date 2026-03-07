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
    <section class="panel relative overflow-visible p-6 lg:p-7">
      <div class="pointer-events-none absolute -right-14 -top-14 h-36 w-36 rounded-full bg-[color:var(--umg-gold-500)]/14 blur-3xl"></div>
      <div class="relative">
        <h2 class="section-title text-2xl">Solicitud de Traslado de Sede</h2>
        <p class="mt-1 text-sm text-muted">Busca tu sede destino, define jornada y valida cupo antes de crear la solicitud.</p>

        <div class="mt-6 grid gap-4 lg:grid-cols-12">
          <div class="lg:col-span-6">
            <label class="mb-1.5 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Sede destino</label>
            <div class="relative">
              <input
                class="input-control pr-20"
                [ngModel]="campusSearch"
                (ngModelChange)="onCampusSearchChange($event)"
                (focus)="openCampusDropdown()"
                (blur)="onCampusInputBlur()"
                placeholder="Escribe para buscar sede o centro"
                autocomplete="off"
              />
              <span class="pointer-events-none absolute right-11 top-1/2 -translate-y-1/2 text-slate-400">
                <svg class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fill-rule="evenodd" d="M9 3a6 6 0 104.472 10.001l2.764 2.763a1 1 0 001.414-1.414l-2.763-2.764A6 6 0 009 3zm-4 6a4 4 0 118 0 4 4 0 01-8 0z" clip-rule="evenodd" />
                </svg>
              </span>
              <button
                type="button"
                class="absolute right-2 top-1/2 -translate-y-1/2 rounded-md border border-slate-200 px-2 py-1 text-[11px] font-semibold text-slate-600 hover:bg-slate-50"
                (mousedown)="$event.preventDefault()"
                (click)="toggleCampusDropdown()"
              >
                {{ campusDropdownOpen ? 'Cerrar' : 'Abrir' }}
              </button>
            </div>

            <div
              *ngIf="campusDropdownOpen"
              class="absolute z-20 mt-1 w-full max-w-[min(100%,33rem)] overflow-hidden rounded-xl border border-slate-200 bg-white shadow-[0_18px_40px_rgba(15,35,61,0.16)]"
            >
              <div class="max-h-64 overflow-y-auto p-1.5">
                <button
                  *ngFor="let campus of filteredCampuses"
                  type="button"
                  class="w-full rounded-lg px-3 py-2 text-left text-sm text-slate-700 transition hover:bg-slate-100"
                  [class.bg-[color:var(--umg-navy-900)]]="campus.id === campusId"
                  [class.text-white]="campus.id === campusId"
                  (mousedown)="$event.preventDefault()"
                  (click)="selectCampus(campus)"
                >
                  <span class="font-semibold">{{ campus.name }}</span>
                  <span class="ml-1 text-xs" [class.text-slate-200]="campus.id === campusId" [class.text-slate-500]="campus.id !== campusId">
                    · {{ campus.campusType }}{{ campus.region ? ' · ' + campus.region : '' }}
                  </span>
                </button>
                <p *ngIf="filteredCampuses.length === 0" class="px-3 py-4 text-center text-sm text-slate-500">
                  No se encontraron sedes con ese criterio.
                </p>
              </div>
            </div>
          </div>

          <div class="lg:col-span-3">
            <label class="mb-1.5 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Jornada</label>
            <select class="input-control" [(ngModel)]="shift">
              <option value="Saturday">Sábado</option>
              <option value="Sunday">Domingo</option>
            </select>
          </div>

          <div class="lg:col-span-3">
            <label class="mb-1.5 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Modalidad</label>
            <select class="input-control" [(ngModel)]="modality">
              <option value="Presencial">Presencial</option>
              <option value="Virtual">Virtual</option>
            </select>
          </div>
        </div>

        <div class="mt-4 grid gap-4 lg:grid-cols-12">
          <div class="lg:col-span-8">
            <label class="mb-1.5 block text-xs font-semibold uppercase tracking-[0.08em] text-slate-600">Observaciones</label>
            <textarea
              class="input-control min-h-[98px]"
              rows="3"
              [(ngModel)]="reason"
              placeholder="Motivo del traslado (opcional)"
            ></textarea>
          </div>

          <div class="flex flex-col justify-end gap-2 lg:col-span-4">
            <button class="btn-secondary px-4 py-2.5 text-sm" (click)="checkAvailability()" [disabled]="campusId <= 0 || checkingAvailability">
              {{ checkingAvailability ? 'Validando cupo...' : 'Validar cupo' }}
            </button>
            <button class="btn-primary px-4 py-2.5 text-sm" (click)="createTransfer()" [disabled]="campusId <= 0 || creatingTransfer">
              {{ creatingTransfer ? 'Creando solicitud...' : 'Crear solicitud' }}
            </button>
          </div>
        </div>

        <div class="mt-4 space-y-2">
          <p *ngIf="availabilityText" class="rounded-lg border border-sky-200 bg-sky-50 px-3 py-2 text-sm text-sky-800">{{ availabilityText }}</p>
          <p *ngIf="message" class="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{{ message }}</p>
          <p *ngIf="error" class="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">{{ error }}</p>
        </div>
      </div>
    </section>

    <section class="panel mt-6 p-6 lg:p-7">
      <div class="mb-4 flex items-center justify-between">
        <h3 class="section-title text-lg">Mis solicitudes de traslado</h3>
        <span class="text-xs font-semibold uppercase tracking-[0.09em] text-slate-500">{{ transfers.length }} registro(s)</span>
      </div>

      <div *ngIf="transfers.length === 0" class="rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-10 text-center">
        <p class="font-display text-lg font-bold text-[color:var(--umg-navy-900)]">Sin solicitudes registradas</p>
        <p class="mt-1 text-sm text-muted">Cuando crees tu primer traslado, aparecerá aquí con su estado y acciones disponibles.</p>
      </div>

      <div *ngIf="transfers.length > 0" class="overflow-x-auto rounded-xl border border-slate-200">
        <table class="table-clean w-full text-left text-sm">
          <thead class="bg-slate-50">
            <tr class="border-b border-slate-200">
              <th class="py-3 pl-4">Fecha</th>
              <th class="py-3">Origen</th>
              <th class="py-3">Destino</th>
              <th class="py-3">Jornada</th>
              <th class="py-3">Modalidad</th>
              <th class="py-3">Estado</th>
              <th class="py-3 pr-4 text-right">Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of transfers" class="border-b border-slate-100 transition hover:bg-slate-50/70">
              <td class="py-3 pl-4">{{ item.createdAt | date:'short' }}</td>
              <td class="py-3">{{ item.fromCampus }}</td>
              <td class="py-3">
                <span class="font-semibold text-[color:var(--umg-navy-900)]">{{ item.toCampus }}</span>
              </td>
              <td class="py-3">{{ item.shift === 'Saturday' ? 'Sábado' : 'Domingo' }}</td>
              <td class="py-3">{{ item.modality }}</td>
              <td class="py-3"><app-status-badge [label]="item.status"></app-status-badge></td>
              <td class="py-3 pr-4 text-right">
                <button
                  *ngIf="item.status === 'PendingPayment'"
                  class="btn-danger px-3 py-1.5 text-xs"
                  (click)="cancelTransfer(item.transferId)"
                >
                  Cancelar
                </button>
                <span *ngIf="item.status !== 'PendingPayment'" class="text-xs text-slate-400">Sin acción</span>
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
  campusSearch = '';
  campusDropdownOpen = false;
  shift: 'Saturday' | 'Sunday' = 'Saturday';
  modality: 'Presencial' | 'Virtual' = 'Presencial';
  reason = '';
  availabilityText = '';
  message = '';
  error = '';
  checkingAvailability = false;
  creatingTransfer = false;

  get filteredCampuses(): CampusResponse[] {
    const term = this.normalizeText(this.campusSearch);
    if (!term) {
      return this.campuses;
    }

    return this.campuses.filter((campus) =>
      this.normalizeText(`${campus.name} ${campus.code} ${campus.campusType} ${campus.region ?? ''}`).includes(term)
    );
  }

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

  onCampusSearchChange(value: string): void {
    this.campusSearch = value;
    this.campusDropdownOpen = true;
    this.error = '';
    this.availabilityText = '';
    this.campusId = 0;
  }

  onCampusInputBlur(): void {
    setTimeout(() => {
      this.campusDropdownOpen = false;
    }, 120);
  }

  openCampusDropdown(): void {
    this.campusDropdownOpen = true;
  }

  toggleCampusDropdown(): void {
    this.campusDropdownOpen = !this.campusDropdownOpen;
  }

  selectCampus(campus: CampusResponse): void {
    this.campusId = campus.id;
    this.campusSearch = `${campus.name} · ${campus.campusType}${campus.region ? ` · ${campus.region}` : ''}`;
    this.campusDropdownOpen = false;
    this.error = '';
  }

  loadMyTransfers(): void {
    this.http.get<ApiEnvelope<TransferResponse[]>>(`${this.baseUrl}/transfers/my`).subscribe((response) => {
      this.transfers = response.success ? response.data : [];
    });
  }

  checkAvailability(): void {
    if (this.campusId <= 0 || this.checkingAvailability) {
      return;
    }

    this.error = '';
    this.availabilityText = '';
    this.checkingAvailability = true;
    this.http
      .get<ApiEnvelope<TransferAvailabilityResponse>>(`${this.baseUrl}/transfers/availability`, {
        params: { campusId: this.campusId, shift: this.shift }
      })
      .subscribe({
        next: (response) => {
          this.checkingAvailability = false;
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo validar disponibilidad.';
            return;
          }

          const data = response.data;
          this.availabilityText = `Cupos: ${data.availableCapacity}/${data.totalCapacity} en ${data.campusName} - ${data.shiftName === 'Saturday' ? 'Sábado' : 'Domingo'}`;
        },
        error: (error: HttpErrorResponse) => {
          this.checkingAvailability = false;
          this.error = error.error?.error?.message ?? 'Error de conexión.';
        }
      });
  }

  createTransfer(): void {
    if (this.campusId <= 0 || this.creatingTransfer) {
      return;
    }

    this.error = '';
    this.message = '';
    this.creatingTransfer = true;

    this.http
      .post<ApiEnvelope<{ paymentOrderId: string; amount: number; currency: string; expiresAt: string }>>(`${this.baseUrl}/transfers`, {
        campusId: this.campusId,
        shift: this.shift,
        modality: this.modality,
        reason: this.reason
      })
      .subscribe({
        next: (response) => {
          this.creatingTransfer = false;
          if (!response.success) {
            this.error = response.error?.message ?? 'No se pudo crear la solicitud.';
            return;
          }

          this.message = `Solicitud creada (${this.modality}). Orden de pago ${response.data.paymentOrderId} por Q${response.data.amount.toFixed(2)} (${response.data.currency}).`;
          this.loadMyTransfers();
        },
        error: (error: HttpErrorResponse) => {
          this.creatingTransfer = false;
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

  private normalizeText(value: string): string {
    return value
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }
}
