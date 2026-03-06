import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span class="rounded-full px-2 py-1 text-xs font-semibold" [class]="cssClass">{{ displayLabel }}</span>`
})
export class StatusBadgeComponent {
  @Input() label = 'Unknown';

  get displayLabel(): string {
    const normalized = this.label.toLowerCase();
    const map: Record<string, string> = {
      pendingpayment: 'Pendiente de Pago',
      pendingreview: 'Pendiente de Revision',
      paid: 'Pagado',
      pending: 'Pendiente',
      approved: 'Aprobado',
      confirmed: 'Confirmado',
      requested: 'Solicitado',
      pdfgenerated: 'PDF Generado',
      sent: 'Enviado',
      rejected: 'Rechazado',
      cancelled: 'Cancelado'
    };

    return map[normalized] ?? this.label;
  }

  get cssClass(): string {
    const normalized = this.label.toLowerCase();
    if (normalized.includes('paid') || normalized.includes('approved') || normalized.includes('confirmed') || normalized.includes('sent')) {
      return 'bg-emerald-100 text-emerald-700';
    }

    if (normalized.includes('pending') || normalized.includes('review') || normalized.includes('requested')) {
      return 'bg-amber-100 text-amber-700';
    }

    return 'bg-rose-100 text-rose-700';
  }
}
