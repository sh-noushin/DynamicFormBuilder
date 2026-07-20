import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Client, FormDto, FormSubmissionDto } from '../../../core/services/api-service';
import { forkJoin, Observable } from 'rxjs';

interface RecentSubmission {
  submittedAt?: Date;
  submitterName?: string;
  submitterEmail?: string;
  formName: string;
  formId?: string;
}

interface TopForm {
  form: FormDto;
  count: number;
}

@Component({
  selector: 'app-admin-overview',
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
  ],
  templateUrl: './admin-overview.component.html',
  styleUrl: './admin-overview.component.scss',
})
export class AdminOverviewComponent {
  private api = inject(Client);
  private router = inject(Router);

  loading = signal(true);
  forms = signal<FormDto[]>([]);
  submissionsByForm = signal<Record<string, FormSubmissionDto[]>>({});

  totalForms = computed(() => this.forms().length);
  activeForms = computed(() => this.forms().filter(f => f.isActive).length);

  allSubmissions = computed<{ sub: FormSubmissionDto; form: FormDto }[]>(() => {
    const map = this.submissionsByForm();
    const out: { sub: FormSubmissionDto; form: FormDto }[] = [];
    for (const form of this.forms()) {
      const subs = map[form.id ?? ''] ?? [];
      for (const s of subs) out.push({ sub: s, form });
    }
    return out;
  });

  totalSubmissions = computed(() => this.allSubmissions().length);

  submissionsLast7Days = computed(() => {
    const cutoff = Date.now() - 7 * 24 * 60 * 60 * 1000;
    return this.allSubmissions().filter(({ sub }) => {
      const t = sub.submittedAt ? new Date(sub.submittedAt).getTime() : 0;
      return t >= cutoff;
    }).length;
  });

  recentSubmissions = computed<RecentSubmission[]>(() =>
    this.allSubmissions()
      .slice()
      .sort((a, b) => {
        const ta = a.sub.submittedAt ? new Date(a.sub.submittedAt).getTime() : 0;
        const tb = b.sub.submittedAt ? new Date(b.sub.submittedAt).getTime() : 0;
        return tb - ta;
      })
      .slice(0, 5)
      .map(({ sub, form }) => ({
        submittedAt: sub.submittedAt,
        submitterName: sub.submitterName,
        submitterEmail: sub.submitterEmail,
        formName: form.name ?? '(unknown form)',
        formId: form.id,
      }))
  );

  topForms = computed<TopForm[]>(() =>
    this.forms()
      .map(form => ({
        form,
        count: (this.submissionsByForm()[form.id ?? ''] ?? []).length,
      }))
      .filter(t => t.count > 0)
      .sort((a, b) => b.count - a.count)
      .slice(0, 3)
  );

  constructor() {
    this.load();
  }

  private load(): void {
    this.api.formsAll().subscribe({
      next: forms => {
        this.forms.set(forms);
        if (forms.length === 0) {
          this.loading.set(false);
          return;
        }
        const requests: Record<string, Observable<FormSubmissionDto[]>> = {};
        for (const f of forms) {
          if (f.id) requests[f.id] = this.api.form(f.id);
        }
        forkJoin(requests).subscribe({
          next: subsMap => {
            this.submissionsByForm.set(subsMap);
            this.loading.set(false);
          },
          error: () => this.loading.set(false),
        });
      },
      error: () => this.loading.set(false),
    });
  }

  viewSubmissions(formId?: string): void {
    if (!formId) return;
    this.router.navigate(['/admin/forms', formId, 'submissions']);
  }
}
