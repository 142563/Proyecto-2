import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span class="rounded-full px-2 py-1 text-xs font-semibold" [class]="cssClass">{{ label }}</span>`
})
export class StatusBadgeComponent {
  @Input() label = 'Unknown';

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
