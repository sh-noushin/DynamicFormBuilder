import { ChangeDetectionStrategy, Component, Inject, computed, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule, MatChipInputEvent } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { FormFieldDto, FormSubmissionDto } from '../../../../core/services/api-service';
import { environment } from '../../../../../environments/environment';

export interface SubmissionDetailDialogData {
  // Ordered list of submissions the caller wants navigable. The detail
  // dialog uses this for its prev/next arrows and updates entries in place
  // when the admin saves notes or tags, so the caller can pick up the
  // mutations on afterClosed().
  submissions: FormSubmissionDto[];
  initialIndex: number;
  fields: FormFieldDto[];
}

interface RenderedField {
  name: string;
  label: string;
  type: string;
  raw: string;
}

@Component({
  selector: 'app-submission-detail-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatChipsModule,
    MatTooltipModule,
    DatePipe,
  ],
  templateUrl: './submission-detail-dialog.component.html',
  styleUrls: ['./submission-detail-dialog.component.scss'],
})
export class SubmissionDetailDialogComponent {
  submission = signal<FormSubmissionDto | null>(null);
  adminNotes = signal<string>('');
  savingNotes = signal(false);
  tags = signal<string[]>([]);
  savingTags = signal(false);
  // Chip input commits on Enter or comma; matches most tag-input UX in the
  // wild and avoids surprising users who paste comma-separated lists.
  readonly separatorKeysCodes = [ENTER, COMMA];

  // Navigation state. index points into data.submissions; ids of rows the
  // dialog has mutated are collected so the caller can update them without
  // re-fetching the whole table.
  index = signal<number>(0);
  private updatedIds = new Set<string>();

  count = computed<number>(() => this.data.submissions?.length ?? 0);
  hasPrev = computed<boolean>(() => this.index() > 0);
  hasNext = computed<boolean>(() => this.index() < this.count() - 1);
  positionLabel = computed<string>(() => `${this.index() + 1} of ${this.count()}`);

  // Field defs in display order with their submitted values resolved. Fields
  // that were never answered still render as an em-dash so the layout stays
  // stable across submissions.
  rows = computed<RenderedField[]>(() => {
    const s = this.submission();
    if (!s) return [];
    const byName = new Map<string, string>();
    for (const v of s.values ?? []) {
      byName.set(String(v.fieldName ?? ''), String(v.fieldValue ?? ''));
    }
    return this.data.fields
      .slice()
      .sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
      .filter(f => String(f.type) !== 'PageBreak')
      .map(f => ({
        name: String(f.name ?? ''),
        label: String(f.label ?? f.name ?? ''),
        type: String(f.type ?? ''),
        raw: byName.get(String(f.name ?? '')) ?? '',
      }));
  });

  constructor(
    private http: HttpClient,
    private snack: MatSnackBar,
    private dialogRef: MatDialogRef<SubmissionDetailDialogComponent, { updatedIds?: string[] } | undefined>,
    @Inject(MAT_DIALOG_DATA) public data: SubmissionDetailDialogData,
  ) {
    const start = Math.max(0, Math.min(data.initialIndex ?? 0, (data.submissions?.length ?? 1) - 1));
    this.index.set(start);
    this.loadCurrent();
  }

  private loadCurrent(): void {
    const current = this.data.submissions[this.index()];
    if (!current) return;
    this.submission.set(current);
    this.adminNotes.set(String((current as any).adminNotes ?? ''));
    const rawTags = (current as any).tags;
    this.tags.set(Array.isArray(rawTags) ? rawTags.map((t: any) => String(t)) : []);
  }

  prev(): void {
    if (!this.hasPrev()) return;
    this.index.set(this.index() - 1);
    this.loadCurrent();
  }

  next(): void {
    if (!this.hasNext()) return;
    this.index.set(this.index() + 1);
    this.loadCurrent();
  }

  addTag(event: MatChipInputEvent) {
    const value = (event.value || '').trim().toLowerCase();
    if (value && !this.tags().includes(value)) {
      const next = [...this.tags(), value];
      this.tags.set(next);
      this.saveTags(next);
    }
    event.chipInput?.clear();
  }

  removeTag(tag: string) {
    const next = this.tags().filter(t => t !== tag);
    this.tags.set(next);
    this.saveTags(next);
  }

  private saveTags(next: string[]) {
    const s = this.submission();
    if (!s?.id) return;
    this.savingTags.set(true);

    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('auth_token') : null;
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();

    this.http
      .patch<FormSubmissionDto>(
        `${environment.apiBaseUrl}/api/FormSubmissions/${encodeURIComponent(String(s.id))}/tags`,
        { tags: next },
        { headers },
      )
      .subscribe({
        next: (updated) => {
          this.savingTags.set(false);
          // Mutate the entry inside data.submissions so prev/next later
          // reflect the fresh tags without a refetch. Caller keeps its own
          // reference to the same array, so the list mutation is visible
          // to the parent when the dialog closes.
          this.applyUpdate(updated);
        },
        error: () => {
          this.savingTags.set(false);
          this.snack.open('Failed to save tags', 'Close', { duration: 3000 });
        },
      });
  }

  private applyUpdate(updated: FormSubmissionDto): void {
    if (!updated?.id) return;
    const idx = this.data.submissions.findIndex(s => String(s.id) === String(updated.id));
    if (idx >= 0) {
      const target = this.data.submissions[idx] as any;
      target.adminNotes = (updated as any).adminNotes;
      target.tags = (updated as any).tags;
      this.updatedIds.add(String(updated.id));
      if (idx === this.index()) this.submission.set(this.data.submissions[idx]);
    }
  }

  setNotesFromEvent(ev: Event) {
    const val = (ev.target as HTMLTextAreaElement)?.value ?? '';
    this.adminNotes.set(val);
  }

  saveNotes() {
    const s = this.submission();
    if (!s?.id || this.savingNotes()) return;
    this.savingNotes.set(true);

    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('auth_token') : null;
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();

    this.http
      .patch<FormSubmissionDto>(
        `${environment.apiBaseUrl}/api/FormSubmissions/${encodeURIComponent(String(s.id))}/notes`,
        { adminNotes: this.adminNotes().trim() || null },
        { headers },
      )
      .subscribe({
        next: (updated) => {
          this.savingNotes.set(false);
          this.applyUpdate(updated);
          this.snack.open('Notes saved', 'Close', { duration: 2000 });
        },
        error: () => {
          this.savingNotes.set(false);
          this.snack.open('Failed to save notes', 'Close', { duration: 3000 });
        },
      });
  }

  fileDownloadUrl(token: string): string {
    return `${environment.apiBaseUrl}/api/uploads/${encodeURIComponent(token)}`;
  }

  fileOriginalName(token: string): string {
    const idx = token.indexOf('__');
    return idx >= 0 ? token.substring(idx + 2) : token;
  }

  ratingStars(value: string): { filled: boolean; index: number }[] {
    const n = Math.max(0, Math.min(5, parseInt(value, 10) || 0));
    return [1, 2, 3, 4, 5].map(i => ({ filled: i <= n, index: i }));
  }

  close() {
    // Hand the caller the ids we touched so it can patch its local table
    // without a full refetch.
    this.dialogRef.close({ updatedIds: Array.from(this.updatedIds) });
  }
}
