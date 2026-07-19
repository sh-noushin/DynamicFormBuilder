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
}

interface PublicForm {
  slug: string;
  name: string;
  description?: string;
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
}
