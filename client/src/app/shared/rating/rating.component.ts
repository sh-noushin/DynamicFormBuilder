import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-rating',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [CommonModule, MatIconModule],
  templateUrl: './rating.component.html',
  styleUrl: './rating.component.scss',
})
export class RatingComponent {
  @Input() label = '';
  @Input() max = 5;
  @Input() value: number | null = null;
  @Output() valueChange = new EventEmitter<number | null>();

  hovered = signal<number | null>(null);

  get stars(): number[] {
    return Array.from({ length: this.max }, (_, i) => i + 1);
  }

  filled(star: number): boolean {
    const active = this.hovered() ?? this.value ?? 0;
    return star <= active;
  }

  select(star: number): void {
    if (this.value === star) {
      this.value = null;
      this.valueChange.emit(null);
    } else {
      this.value = star;
      this.valueChange.emit(star);
    }
  }

  onEnter(star: number): void { this.hovered.set(star); }
  onLeave(): void { this.hovered.set(null); }
}
