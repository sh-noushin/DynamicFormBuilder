import { ChangeDetectionStrategy, Component, Inject, computed, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormFieldDto, FormSubmissionDto } from '../../../../core/services/api-service';
import { environment } from '../../../../../environments/environment';

export interface SubmissionDetailDialogData {
  submission: FormSubmissionDto;
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
    DatePipe,
  ],
  templateUrl: './submission-detail-dialog.component.html',
  styleUrls: ['./submission-detail-dialog.component.scss'],
})
export class SubmissionDetailDialogComponent {
  submission = signal<FormSubmissionDto | null>(null);
  adminNotes = signal<string>('');
  savingNotes = signal(false);

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
    private dialogRef: MatDialogRef<SubmissionDetailDialogComponent, { updated?: FormSubmissionDto } | undefined>,
    @Inject(MAT_DIALOG_DATA) public data: SubmissionDetailDialogData,
  ) {
    this.submission.set(data.submission);
    this.adminNotes.set(String((data.submission as any).adminNotes ?? ''));
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
          this.submission.set(updated);
          this.snack.open('Notes saved', 'Close', { duration: 2000 });
          this.dialogRef.close({ updated });
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

  close() { this.dialogRef.close(); }
}
