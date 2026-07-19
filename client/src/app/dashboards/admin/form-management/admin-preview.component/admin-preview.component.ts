import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Client, FormDto, FormFieldDto, FormVersionDto } from '../../../../core/services/api-service';
import { SignaturePadComponent } from '../../../../shared/signature-pad/signature-pad.component';
import { RatingComponent } from '../../../../shared/rating/rating.component';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-admin-preview',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSelectModule,
    MatSnackBarModule,
    SignaturePadComponent,
    RatingComponent,
  ],
  templateUrl: './admin-preview.component.html',
  styleUrl: './admin-preview.component.scss',
})
export class AdminPreviewComponent {
  private route = inject(ActivatedRoute);
  private api = inject(Client);
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  private snack = inject(MatSnackBar);

  form = signal<FormDto | null>(null);
  version = signal<FormVersionDto | null>(null);
  fields = signal<FormFieldDto[]>([]);
  loading = signal(true);
  errorMessage = signal<string | null>(null);
  fileUploading = signal<Record<string, boolean>>({});
  fileMeta = signal<Record<string, { token: string; name: string; size: number } | undefined>>({});
  formGroup: FormGroup = this.fb.group({});

  formId = computed(() => this.route.snapshot.paramMap.get('formId') ?? '');
  versionNumber = computed(() => Number(this.route.snapshot.paramMap.get('versionNumber') ?? '0'));

  constructor() {
    this.load();
  }

  private load(): void {
    const id = this.formId();
    const vn = this.versionNumber();
    if (!id || !vn) {
      this.errorMessage.set('Missing form id or version.');
      this.loading.set(false);
      return;
    }

    // The Edit form dialog's Preview button always includes ?brandColor=... so
    // the admin can see the color they're currently choosing before hitting
    // Save. Param presence (even with an empty value) means "override the DB
    // value"; empty value means "no brand color - render the default". If the
    // admin navigates directly to the preview URL with no query string we fall
    // back to whatever the DB has.
    const hasOverride = this.route.snapshot.queryParamMap.has('brandColor');
    const overrideColor = this.route.snapshot.queryParamMap.get('brandColor') ?? '';

    this.api.formsGET(id).subscribe({
      next: form => {
        if (hasOverride) {
          (form as any).brandColor = overrideColor || null;
        }
        this.form.set(form);
        const match = (form.versions ?? []).find(v => v.versionNumber === vn) ?? null;
        this.version.set(match);
      },
      error: () => this.errorMessage.set('Could not load form.'),
    });

    this.api.fieldsAll(id, vn).subscribe({
      next: fields => {
        const sorted = [...fields].sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
        this.fields.set(sorted);
        this.buildFormControls(sorted);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load fields.');
        this.loading.set(false);
      },
    });
  }

  private buildFormControls(fields: FormFieldDto[]): void {
    for (const f of fields) {
      const validators = f.isRequired && f.type !== 'Checkbox' ? [Validators.required] : [];
      if (f.type === 'Email') validators.push(Validators.email);
      const initial = f.type === 'Checkbox' ? false : (f.defaultValue ?? '');
      this.formGroup.addControl(this.controlName(f), this.fb.control(initial, validators));
    }
  }

  controlName(f: FormFieldDto): string {
    return `field_${f.id}`;
  }

  getOptions(f: FormFieldDto): Array<{ value: string; label: string }> {
    const raw = f.options as string | undefined;
    if (!raw) return [];
    try {
      const parsed = JSON.parse(raw);
      if (Array.isArray(parsed)) {
        return parsed.map((o: any) => ({
          value: String(o?.value ?? ''),
          label: String(o?.label ?? o?.value ?? ''),
        }));
      }
    } catch { /* fall through */ }
    return raw.split(',').map(s => s.trim()).filter(Boolean).map(s => ({ value: s, label: s }));
  }

  isFieldVisible(f: FormFieldDto): boolean {
    const raw = (f as any).showIfCondition as string | undefined;
    if (!raw) return true;
    let rule: { field?: string; equals?: unknown } | null = null;
    try { rule = JSON.parse(raw); } catch { return true; }
    if (!rule?.field) return true;
    const target = this.fields().find(x => x.name === rule!.field);
    if (!target) return true;
    const value = this.formGroup.get(this.controlName(target))?.value;
    return String(value ?? '') === String(rule.equals ?? '');
  }

  onSignatureChange(f: FormFieldDto, dataUrl: string | null): void {
    this.formGroup.get(this.controlName(f))?.setValue(dataUrl ?? '');
  }

  onRatingChange(f: FormFieldDto, value: number | null): void {
    this.formGroup.get(this.controlName(f))?.setValue(value != null ? String(value) : '');
  }

  uploadFile(f: FormFieldDto, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const name = this.controlName(f);
    this.fileUploading.update(m => ({ ...m, [name]: true }));
    const data = new FormData();
    data.append('file', file);
    this.http.post<{ token: string; originalFileName: string; sizeBytes: number }>(
      `${environment.apiBaseUrl}/api/uploads`, data,
    ).subscribe({
      next: r => {
        this.fileUploading.update(m => ({ ...m, [name]: false }));
        this.fileMeta.update(m => ({ ...m, [name]: { token: r.token, name: r.originalFileName, size: r.sizeBytes } }));
        this.formGroup.get(name)?.setValue(r.token);
      },
      error: () => {
        this.fileUploading.update(m => ({ ...m, [name]: false }));
        this.snack.open('File upload failed', 'Close', { duration: 3000 });
        input.value = '';
      },
    });
  }

  testSubmit(): void {
    this.formGroup.markAllAsTouched();
    if (this.formGroup.invalid) {
      this.snack.open('Preview: fix the highlighted fields to proceed', 'Close', { duration: 3000 });
      return;
    }
    this.snack.open('Preview: submission would be accepted (nothing saved)', 'Close', { duration: 3000 });
  }
}
