import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Client, FormDto } from '../../../../core/services/api-service';
import { environment } from '../../../../../environments/environment';

interface DailyCount {
  date: string;
  count: number;
}

interface FormAnalytics {
  totalSubmissions: number;
  last7Days: number;
  last30Days: number;
  dailyCounts: DailyCount[];
}

// Layout constants for the inline SVG chart. All in unitless SVG "user units"
// so the outer viewBox scales the whole thing to the container width.
const CHART_WIDTH = 720;
const CHART_HEIGHT = 220;
const CHART_PAD_LEFT = 32;
const CHART_PAD_RIGHT = 8;
const CHART_PAD_TOP = 12;
const CHART_PAD_BOTTOM = 28;

@Component({
  selector: 'app-form-analytics',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    DatePipe,
    DecimalPipe,
  ],
  templateUrl: './form-analytics.component.html',
  styleUrl: './form-analytics.component.scss',
})
export class FormAnalyticsComponent {
  private route = inject(ActivatedRoute);
  private api = inject(Client);
  private http = inject(HttpClient);

  form = signal<FormDto | null>(null);
  analytics = signal<FormAnalytics | null>(null);
  loading = signal(true);
  errorMessage = signal<string | null>(null);

  formId = computed(() => this.route.snapshot.paramMap.get('id') ?? '');

  chartWidth = CHART_WIDTH;
  chartHeight = CHART_HEIGHT;
  viewBox = `0 0 ${CHART_WIDTH} ${CHART_HEIGHT}`;

  // Peak count in the visible range; drives the y-axis and bar heights.
  // Minimum of 1 so an all-zero chart still renders bars with a floor.
  peak = computed<number>(() => {
    const a = this.analytics();
    if (!a) return 1;
    return Math.max(1, ...a.dailyCounts.map(d => d.count));
  });

  bars = computed<Array<{ x: number; y: number; width: number; height: number; date: string; count: number }>>(() => {
    const a = this.analytics();
    if (!a || a.dailyCounts.length === 0) return [];
    const usableWidth = CHART_WIDTH - CHART_PAD_LEFT - CHART_PAD_RIGHT;
    const usableHeight = CHART_HEIGHT - CHART_PAD_TOP - CHART_PAD_BOTTOM;
    const slot = usableWidth / a.dailyCounts.length;
    const barWidth = Math.max(2, slot - 2);
    const p = this.peak();
    return a.dailyCounts.map((d, i) => {
      const height = (d.count / p) * usableHeight;
      return {
        x: CHART_PAD_LEFT + i * slot + (slot - barWidth) / 2,
        y: CHART_PAD_TOP + (usableHeight - height),
        width: barWidth,
        height,
        date: d.date,
        count: d.count,
      };
    });
  });

  // Sparse x-axis labels: first, middle, last day of the range.
  xLabels = computed<Array<{ x: number; label: string }>>(() => {
    const a = this.analytics();
    if (!a || a.dailyCounts.length === 0) return [];
    const usableWidth = CHART_WIDTH - CHART_PAD_LEFT - CHART_PAD_RIGHT;
    const slot = usableWidth / a.dailyCounts.length;
    const format = (iso: string) => {
      const d = new Date(iso + 'T00:00:00');
      return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
    };
    const picks = [0, Math.floor(a.dailyCounts.length / 2), a.dailyCounts.length - 1]
      .filter((v, i, arr) => arr.indexOf(v) === i);
    return picks.map(i => ({
      x: CHART_PAD_LEFT + i * slot + slot / 2,
      label: format(a.dailyCounts[i].date),
    }));
  });

  yTop = computed(() => CHART_PAD_TOP);
  yBottom = computed(() => CHART_HEIGHT - CHART_PAD_BOTTOM);
  chartInnerLeft = CHART_PAD_LEFT;
  chartInnerRight = CHART_WIDTH - CHART_PAD_RIGHT;

  constructor() {
    this.load();
  }

  private load(): void {
    const id = this.formId();
    if (!id) {
      this.errorMessage.set('Missing form id.');
      this.loading.set(false);
      return;
    }

    this.api.formsGET(id).subscribe({
      next: form => this.form.set(form),
      error: () => this.errorMessage.set('Could not load form.'),
    });

    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('auth_token') : null;
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();
    this.http
      .get<FormAnalytics>(`${environment.apiBaseUrl}/api/forms/${id}/analytics?days=30`, { headers })
      .subscribe({
        next: a => {
          this.analytics.set(a);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('Could not load analytics.');
          this.loading.set(false);
        },
      });
  }
}
