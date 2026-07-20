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
  thankYouMessage?: string;
  redirectUrl?: string;
  requiresPassword?: boolean;
  isClosed?: boolean;
  closedReason?: string;
  formVersionId: string;
  versionNumber: number;
  fields: PublicField[];
}

const PASSWORD_HEADER = 'X-Form-Password';

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
  passwordRequired = signal(false);
  formClosed = signal(false);
  closedReason = signal<string | null>(null);
  passwordValue = signal('');
  passwordAttempted = signal(false);
  passwordError = signal<string | null>(null);
  passwordSubmitting = signal(false);
  fieldErrors = signal<Record<string, string[]>>({});
  fileUploading = signal<Record<string, boolean>>({});
  fileMeta = signal<Record<string, { token: string; name: string; size: number } | undefined>>({});
  currentPage = signal(0);
  formGroup: FormGroup = this.fb.group({
    submitterName: [''],
    submitterEmail: [''],
    // Honeypot: real users never see or type in this control. Any non-empty
    // value on submit is treated as bot traffic by the server (400) and by
    // the client as a silent no-op.
    _hp: [''],
  });

  slug = computed(() => this.route.snapshot.paramMap.get('slug') ?? '');

  // Split the ordered field list into pages at every PageBreak marker.
  // The PageBreak itself is not rendered as an input; its label becomes the
  // heading for the page AFTER the break (page[i+1]).
  pages = computed<PublicField[][]>(() => {
    const fields = this.form()?.fields ?? [];
    if (fields.length === 0) return [[]];
    const result: PublicField[][] = [[]];
    for (const f of fields) {
      if (f.type === 'PageBreak') {
        result.push([]);
      } else {
        result[result.length - 1].push(f);
      }
    }
    return result;
  });

  pageHeadings = computed<string[]>(() => {
    const fields = this.form()?.fields ?? [];
    const headings: string[] = [''];
    for (const f of fields) {
      if (f.type === 'PageBreak') headings.push(f.label || '');
    }
    return headings;
  });

  currentPageFields = computed<PublicField[]>(() => this.pages()[this.currentPage()] ?? []);
  totalPages = computed(() => this.pages().length);
  isLastPage = computed(() => this.currentPage() >= this.totalPages() - 1);
  isFirstPage = computed(() => this.currentPage() === 0);
  progressPercent = computed(() => {
    const t = this.totalPages();
    if (t <= 1) return 100;
    return Math.round(((this.currentPage() + 1) / t) * 100);
  });

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

    const headers = this.passwordValue()
      ? { [PASSWORD_HEADER]: this.passwordValue() }
      : undefined;

    this.http
      .get<PublicForm>(`${environment.apiBaseUrl}/api/public/forms/${encodeURIComponent(slug)}`, { headers })
      .subscribe({
        next: (form) => {
          this.form.set(form);
          this.loading.set(false);
          this.passwordSubmitting.set(false);
          if (form.isClosed) {
            this.formClosed.set(true);
            this.closedReason.set(form.closedReason ?? 'This form is no longer accepting responses.');
            return;
          }
          this.formClosed.set(false);
          this.closedReason.set(null);
          if (form.requiresPassword) {
            this.passwordRequired.set(true);
            if (this.passwordAttempted()) {
              this.passwordError.set('Incorrect password. Please try again.');
            }
            return;
          }
          this.passwordRequired.set(false);
          this.passwordError.set(null);
          this.buildFormControls(form);
        },
        error: (err) => {
          this.errorMessage.set(
            err?.status === 404
              ? 'This form does not exist or is no longer accepting responses.'
              : 'We could not load this form. Please try again.'
          );
          this.loading.set(false);
          this.passwordSubmitting.set(false);
        },
      });
  }

  setPasswordFromEvent(ev: Event): void {
    const val = (ev.target as HTMLInputElement)?.value ?? '';
    this.passwordValue.set(val);
    if (this.passwordError()) this.passwordError.set(null);
  }

  unlockForm(): void {
    if (this.passwordSubmitting()) return;
    const value = this.passwordValue().trim();
    if (!value) {
      this.passwordError.set('Please enter the password.');
      return;
    }
    this.passwordAttempted.set(true);
    this.passwordSubmitting.set(true);
    this.passwordError.set(null);
    this.loading.set(true);
    this.load();
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

  nextPage(): void {
    // Only advance if the visible fields on the current page validate.
    const currentControls = this.currentPageFields()
      .filter(f => this.isFieldVisible(f))
      .map(f => this.controlName(f));
    let anyInvalid = false;
    for (const name of currentControls) {
      const ctrl = this.formGroup.get(name);
      if (ctrl) {
        ctrl.markAsTouched();
        if (ctrl.invalid) anyInvalid = true;
      }
    }
    if (anyInvalid) {
      this.errorMessage.set('Please fix the highlighted fields to continue.');
      return;
    }
    this.errorMessage.set(null);
    if (!this.isLastPage()) {
      this.currentPage.update(p => p + 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  previousPage(): void {
    if (!this.isFirstPage()) {
      this.errorMessage.set(null);
      this.currentPage.update(p => p - 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
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

    const headers = this.passwordValue()
      ? { [PASSWORD_HEADER]: this.passwordValue() }
      : undefined;

    this.http
      .post(`${environment.apiBaseUrl}/api/public/forms/${encodeURIComponent(form.slug)}/submissions`, {
        submitterName: (raw['submitterName'] as string | null) || null,
        submitterEmail: (raw['submitterEmail'] as string | null) || null,
        fieldValues,
        honeypotValue: (raw['_hp'] as string | null) || null,
      }, { headers })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          const target = form.redirectUrl?.trim();
          if (target && this.isSafeRedirect(target)) {
            // External redirect - full-page navigation, not Angular router.
            window.location.assign(target);
            return;
          }
          this.submitted.set(true);
        },
        error: (err) => {
          this.submitting.set(false);
          if (err?.status === 400 && err.error?.fieldErrors) {
            this.fieldErrors.set(err.error.fieldErrors as Record<string, string[]>);
            this.errorMessage.set('Please fix the highlighted fields and submit again.');
          } else if (err?.status === 401) {
            this.errorMessage.set('The form password has changed. Please reload and enter it again.');
            this.passwordRequired.set(true);
            this.passwordValue.set('');
          } else if (err?.status === 410) {
            this.formClosed.set(true);
            this.closedReason.set(err?.error?.message ?? err?.error?.detail ?? 'This form is no longer accepting responses.');
          } else if (err?.status === 409) {
            this.errorMessage.set(err?.error?.message ?? err?.error?.detail ?? 'A response has already been submitted from this email address.');
          } else if (err?.status === 429) {
            this.errorMessage.set('You are submitting too quickly. Please wait a moment and try again.');
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

  // Belt-and-braces client check even though the server also validates.
  // Only allow absolute http/https targets - blocks javascript:, data:, etc.
  private isSafeRedirect(url: string): boolean {
    try {
      const u = new URL(url);
      return u.protocol === 'http:' || u.protocol === 'https:';
    } catch {
      return false;
    }
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

    // Client-side pre-check against the field's Validation JSON. The server
    // enforces the same rules; this just gives immediate feedback without
    // waiting for a round trip.
    const localError = this.checkFileAgainstField(field, file);
    if (localError) {
      this.errorMessage.set(localError);
      input.value = '';
      return;
    }

    this.fileUploading.update(m => ({ ...m, [name]: true }));
    const data = new FormData();
    data.append('file', file);

    // Pass slug + field name so the server can enforce per-field constraints.
    const slug = encodeURIComponent(this.slug());
    const fieldName = encodeURIComponent(field.name);
    const url = `${environment.apiBaseUrl}/api/uploads?slug=${slug}&fieldName=${fieldName}`;

    this.http.post<{ token: string; originalFileName: string; sizeBytes: number }>(url, data).subscribe({
      next: r => {
        this.fileUploading.update(m => ({ ...m, [name]: false }));
        this.fileMeta.update(m => ({ ...m, [name]: { token: r.token, name: r.originalFileName, size: r.sizeBytes } }));
        this.formGroup.get(name)?.setValue(r.token);
      },
      error: (err) => {
        this.fileUploading.update(m => ({ ...m, [name]: false }));
        const serverMessage = err?.error?.message ?? err?.error?.detail;
        this.errorMessage.set(serverMessage ?? 'File upload failed. Please try a smaller file.');
        input.value = '';
      }
    });
  }

  // Reads maxFileSizeMb and allowedFileExtensions from the field's Validation
  // JSON and returns a user-facing error string when the file violates either.
  private checkFileAgainstField(field: PublicField, file: File): string | null {
    if (!field.validation) return null;
    let rules: { maxFileSizeMb?: number; allowedFileExtensions?: string[] } | null = null;
    try {
      rules = JSON.parse(field.validation);
    } catch {
      return null;
    }
    if (!rules) return null;

    if (typeof rules.maxFileSizeMb === 'number' && rules.maxFileSizeMb > 0) {
      const maxBytes = rules.maxFileSizeMb * 1024 * 1024;
      if (file.size > maxBytes) {
        return `File exceeds the maximum size of ${rules.maxFileSizeMb} MB for this field.`;
      }
    }
    if (Array.isArray(rules.allowedFileExtensions) && rules.allowedFileExtensions.length > 0) {
      const allowed = rules.allowedFileExtensions
        .map(s => String(s).trim().replace(/^\./, '').toLowerCase())
        .filter(Boolean);
      const dot = file.name.lastIndexOf('.');
      const ext = dot >= 0 ? file.name.slice(dot + 1).toLowerCase() : '';
      if (!allowed.includes(ext)) {
        return `File type '.${ext}' is not allowed. Allowed: ${allowed.map(a => '.' + a).join(', ')}.`;
      }
    }
    return null;
  }
}
