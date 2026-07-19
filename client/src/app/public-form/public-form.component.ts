import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SignaturePadComponent } from '../shared/signature-pad/signature-pad.component';
import { RatingComponent } from '../shared/rating/rating.component';
import { environment } from '../../environments/environment';

interface PublicField {
  id: string;
  name: string;
  label: string;
  type: string;
  isRequired: boolean;
  placeholder?: string;
  helpText?: string;
  defaultValue?: string;
  options?: string;
  validation?: string;
  showIfCondition?: string;
}

interface PublicForm {
  slug: string;
  name: string;
  description?: string;
  brandColor?: string;
  formVersionId: string;
  versionNumber: number;
  fields: PublicField[];
}

@Component({
  selector: 'app-public-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatRadioModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    SignaturePadComponent,
    RatingComponent,
  ],
  templateUrl: './public-form.component.html',
  styleUrl: './public-form.component.scss',
})
export class PublicFormComponent {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);

  form = signal<PublicForm | null>(null);
  loading = signal(true);
  submitting = signal(false);
  submitted = signal(false);
  errorMessage = signal<string | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});
  fileUploading = signal<Record<string, boolean>>({});
  fileMeta = signal<Record<string, { token: string; name: string; size: number } | undefined>>({});
  formGroup: FormGroup = this.fb.group({
    submitterName: [''],
    submitterEmail: [''],
  });

  slug = computed(() => this.route.snapshot.paramMap.get('slug') ?? '');

  constructor() {
    this.load();
  }

  private load(): void {
    const slug = this.slug();
    if (!slug) {
      this.errorMessage.set('This link is not valid.');
      this.loading.set(false);
      return;
    }

    this.http
      .get<PublicForm>(`${environment.apiBaseUrl}/api/public/forms/${encodeURIComponent(slug)}`)
      .subscribe({
        next: (form) => {
          this.form.set(form);
          this.buildFormControls(form);
          this.loading.set(false);
        },
        error: (err) => {
          this.errorMessage.set(
            err?.status === 404
              ? 'This form does not exist or is no longer accepting responses.'
              : 'We could not load this form. Please try again.'
          );
          this.loading.set(false);
        },
      });
  }

  private buildFormControls(form: PublicForm): void {
    for (const field of form.fields) {
      const validators = field.isRequired && field.type !== 'Checkbox' ? [Validators.required] : [];
      if (field.type === 'Email') validators.push(Validators.email);
      const initial = this.initialValueFor(field);
      this.formGroup.addControl(this.controlName(field), this.fb.control(initial, validators));
    }
  }

  private initialValueFor(field: PublicField): unknown {
    if (field.type === 'Checkbox') return false;
    return field.defaultValue ?? '';
  }

  controlName(field: PublicField): string {
    return `field_${field.id}`;
  }

  getOptions(field: PublicField): Array<{ value: string; label: string }> {
    if (!field.options) return [];
    try {
      const parsed = JSON.parse(field.options);
      if (Array.isArray(parsed)) {
        return parsed.map((opt: unknown) => {
          if (typeof opt === 'string') return { value: opt, label: opt };
          const o = opt as { value?: string; label?: string };
          return { value: o.value ?? '', label: o.label ?? o.value ?? '' };
        });
      }
    } catch {
      // Fall through to comma-split.
    }
    return field.options
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean)
      .map((s) => ({ value: s, label: s }));
  }

  submit(): void {
    const form = this.form();
    if (!form || this.submitting()) return;
    this.formGroup.markAllAsTouched();
    if (this.formGroup.invalid) return;

    this.submitting.set(true);
    this.fieldErrors.set({});
    this.errorMessage.set(null);

    const raw = this.formGroup.getRawValue() as Record<string, unknown>;
    const fieldValues: Record<string, string | null> = {};
    for (const field of form.fields) {
      if (!this.isFieldVisible(field)) continue;
      const value = raw[this.controlName(field)];
      fieldValues[field.name] = value === undefined || value === null ? null : String(value);
    }

    this.http
      .post(`${environment.apiBaseUrl}/api/public/forms/${encodeURIComponent(form.slug)}/submissions`, {
        submitterName: (raw['submitterName'] as string | null) || null,
        submitterEmail: (raw['submitterEmail'] as string | null) || null,
        fieldValues,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.submitted.set(true);
        },
        error: (err) => {
          this.submitting.set(false);
          if (err?.status === 400 && err.error?.fieldErrors) {
            this.fieldErrors.set(err.error.fieldErrors as Record<string, string[]>);
            this.errorMessage.set('Please fix the highlighted fields and submit again.');
          } else if (err?.status === 404) {
            this.errorMessage.set('This form is no longer accepting responses.');
          } else {
            this.errorMessage.set('Something went wrong sending your response.');
          }
        },
      });
  }

  errorsForField(field: PublicField): string[] {
    return this.fieldErrors()[field.name] ?? [];
  }

  // Evaluates a field's showIfCondition against the current form values.
  // Format: {"field":"otherFieldName","equals":"value"}. Missing/invalid rule = visible.
  isFieldVisible(field: PublicField): boolean {
    if (!field.showIfCondition) return true;
    let rule: { field?: string; equals?: unknown } | null = null;
    try {
      rule = JSON.parse(field.showIfCondition);
    } catch {
      return true;
    }
    if (!rule?.field) return true;
    const form = this.form();
    if (!form) return true;
    const target = form.fields.find(f => f.name === rule!.field);
    if (!target) return true;
    const value = this.formGroup.get(this.controlName(target))?.value;
    return String(value ?? '') === String(rule.equals ?? '');
  }

  onSignatureChange(field: PublicField, dataUrl: string | null) {
    this.formGroup.get(this.controlName(field))?.setValue(dataUrl ?? '');
  }

  onRatingChange(field: PublicField, value: number | null) {
    this.formGroup.get(this.controlName(field))?.setValue(value != null ? String(value) : '');
  }

  uploadFile(field: PublicField, event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const name = this.controlName(field);

    this.fileUploading.update(m => ({ ...m, [name]: true }));
    const data = new FormData();
    data.append('file', file);

    this.http.post<{ token: string; originalFileName: string; sizeBytes: number }>(
      `${environment.apiBaseUrl}/api/uploads`,
      data
    ).subscribe({
      next: r => {
        this.fileUploading.update(m => ({ ...m, [name]: false }));
        this.fileMeta.update(m => ({ ...m, [name]: { token: r.token, name: r.originalFileName, size: r.sizeBytes } }));
        this.formGroup.get(name)?.setValue(r.token);
      },
      error: () => {
        this.fileUploading.update(m => ({ ...m, [name]: false }));
        this.errorMessage.set('File upload failed. Please try a smaller file.');
        input.value = '';
      }
    });
  }
}
