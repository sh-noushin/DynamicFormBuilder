import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Client, FormDto, FormFieldDto, FormSubmissionDto, FormVersionDto } from '../../../../core/services/api-service';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-admin-submissions',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    DatePipe,
  ],
  templateUrl: './admin-submissions.component.html',
  styleUrl: './admin-submissions.component.scss',
})
export class AdminSubmissionsComponent {
  private route = inject(ActivatedRoute);
  private api = inject(Client);
  private http = inject(HttpClient);
  private snack = inject(MatSnackBar);

  form = signal<FormDto | null>(null);
  submissions = signal<FormSubmissionDto[]>([]);
  loading = signal(true);
  downloading = signal(false);
  errorMessage = signal<string | null>(null);
  searchQuery = signal<string>('');
  pageIndex = signal(0);
  pageSize = signal(25);

  formId = computed(() => this.route.snapshot.paramMap.get('id') ?? '');

  filteredSubmissions = computed<FormSubmissionDto[]>(() => {
    const q = this.searchQuery().trim().toLowerCase();
    if (!q) return this.submissions();
    return this.submissions().filter(s => {
      if ((s.submitterName ?? '').toLowerCase().includes(q)) return true;
      if ((s.submitterEmail ?? '').toLowerCase().includes(q)) return true;
      for (const v of (s.values ?? [])) {
        if ((v.fieldValue ?? '').toLowerCase().includes(q)) return true;
      }
      return false;
    });
  });

  pagedSubmissions = computed<FormSubmissionDto[]>(() => {
    const start = this.pageIndex() * this.pageSize();
    return this.filteredSubmissions().slice(start, start + this.pageSize());
  });

  currentVersion = computed<FormVersionDto | null>(() => {
    const f = this.form();
    if (!f) return null;
    return (f.versions ?? []).find(v => v.isCurrentVersion) ?? f.currentVersion ?? null;
  });

  fieldColumns = computed<string[]>(() =>
    (this.currentVersion()?.fields ?? [])
      .slice()
      .sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
      .map(f => f.name ?? '')
      .filter(Boolean)
  );

  displayedColumns = computed(() => ['submittedAt', 'submitterName', 'submitterEmail', ...this.fieldColumns()]);

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

    this.api.form(id).subscribe({
      next: subs => {
        this.submissions.set(subs);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load submissions.');
        this.loading.set(false);
      },
    });
  }

  valueFor(submission: FormSubmissionDto, fieldName: string): string {
    const values = submission.values ?? [];
    const hit = values.find(v => (v.fieldName ?? '') === fieldName);
    return hit?.fieldValue ?? '';
  }

  fieldType(fieldName: string): string {
    const field = (this.currentVersion()?.fields ?? []).find(f => f.name === fieldName);
    return String(field?.type ?? '');
  }

  ratingStars(value: string): { filled: boolean; index: number }[] {
    const n = Math.max(0, Math.min(5, parseInt(value, 10) || 0));
    return [1, 2, 3, 4, 5].map(i => ({ filled: i <= n, index: i }));
  }

  fileDownloadUrl(token: string): string {
    return `${environment.apiBaseUrl}/api/uploads/${encodeURIComponent(token)}`;
  }

  fileOriginalName(token: string): string {
    const idx = token.indexOf('__');
    return idx >= 0 ? token.substring(idx + 2) : token;
  }

  setSearch(value: string): void {
    this.searchQuery.set(value);
    this.pageIndex.set(0);
  }
  clearSearch(): void { this.setSearch(''); }
  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
  }

  downloadCsv(): void {
    const id = this.formId();
    if (!id || this.downloading()) return;
    this.downloading.set(true);

    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('auth_token') : null;
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();

    this.http
      .get(`${environment.apiBaseUrl}/api/FormSubmissions/form/${encodeURIComponent(id)}/export.csv`, {
        responseType: 'blob',
        headers,
        observe: 'response',
      })
      .subscribe({
        next: response => {
          this.downloading.set(false);
          const contentDisposition = response.headers.get('content-disposition') ?? '';
          const match = /filename="?([^";]+)"?/.exec(contentDisposition);
          const filename = match?.[1] ?? `submissions-${id}.csv`;
          const url = URL.createObjectURL(response.body as Blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = filename;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          URL.revokeObjectURL(url);
        },
        error: () => {
          this.downloading.set(false);
          this.snack.open('CSV download failed', 'Close', { duration: 3000 });
        },
      });
  }
}
