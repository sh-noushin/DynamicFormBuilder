import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  output,
  signal,
} from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import {
  FbField,
  FbFormBody,
  FbSchemaError,
  FB_RENDERABLE_TYPES,
  normalizeSchema,
} from './form-schema';
import {
  controlNameFor,
  errorMessageFor,
  initialValueFor,
  isFieldVisible,
  parseOptions,
  validatorsFor,
} from './field-utils';

export interface FbSubmitEvent {
  // Answers keyed by field name — the same keys FormBuilder's API uses.
  data: Record<string, string | null>;
  // Present only when submitToSlug was set and the POST succeeded.
  posted: boolean;
}

export interface FbSubmitErrorEvent {
  status: number | null;
  message: string;
}

// Renders a FormBuilder form from its exported JSON.
//
// Two modes:
//   1. Standalone  - no server involved. (submitted) hands you the answers.
//   2. Post-back   - set apiBaseUrl + submitToSlug and answers are POSTed to
//                    FormBuilder, landing in the workspace dashboard.
//
// Deliberately uses fetch() rather than HttpClient so a consuming app is not
// forced to call provideHttpClient() just to render a form.
@Component({
  selector: 'fb-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './fb-form.component.html',
  styleUrl: './fb-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbFormComponent {
  /** Exported JSON: a full export, a form body, or a bare field array. */
  schema = input.required<unknown>();
  /** e.g. 'https://forms.example.com'. Required only for post-back mode. */
  apiBaseUrl = input<string | null>(null);
  /** Form slug in FormBuilder. When set, answers are POSTed there. */
  submitToSlug = input<string | null>(null);
  submitLabel = input<string>('Submit');
  /** Collect submitter name/email, as the hosted public form does. */
  collectSubmitter = input<boolean>(false);

  submitted = output<FbSubmitEvent>();
  validationFailed = output<void>();
  submitError = output<FbSubmitErrorEvent>();

  private fb = new FormBuilder();
  formGroup: FormGroup = this.fb.group({});

  body = signal<FbFormBody | null>(null);
  schemaError = signal<string | null>(null);
  submitting = signal(false);
  isDone = signal(false);
  currentPage = signal(0);
  // Bumped whenever a control value changes, so visibility computeds
  // re-evaluate. Reactive forms are not signal-based, so this is the bridge.
  private revision = signal(0);

  constructor() {
    effect(() => {
      const raw = this.schema();
      this.rebuild(raw);
    });

    this.formGroup.valueChanges.subscribe(() => this.revision.update((n) => n + 1));
  }

  // -- schema -> controls ---------------------------------------------------

  private rebuild(raw: unknown): void {
    this.isDone.set(false);
    this.currentPage.set(0);

    let normalized: FbFormBody;
    try {
      normalized = normalizeSchema(raw);
    } catch (err) {
      this.body.set(null);
      this.schemaError.set(
        err instanceof FbSchemaError ? err.message : 'Could not read this schema.'
      );
      return;
    }

    this.schemaError.set(null);
    this.body.set(normalized);

    for (const name of Object.keys(this.formGroup.controls)) {
      this.formGroup.removeControl(name, { emitEvent: false });
    }

    if (this.collectSubmitter()) {
      this.formGroup.addControl('submitterName', this.fb.control(''), { emitEvent: false });
      this.formGroup.addControl('submitterEmail', this.fb.control(''), { emitEvent: false });
    }

    for (const field of normalized.fields) {
      if (field.type === 'PageBreak') continue;
      this.formGroup.addControl(
        controlNameFor(field),
        this.fb.control(initialValueFor(field), validatorsFor(field)),
        { emitEvent: false }
      );
    }
    this.revision.update((n) => n + 1);
  }

  // -- pagination -----------------------------------------------------------

  // PageBreak splits the field list into pages. HiddenField keeps its control
  // (its value is submitted) but never occupies a slot on the page.
  pages = computed<FbField[][]>(() => {
    const fields = this.body()?.fields ?? [];
    if (fields.length === 0) return [[]];
    const result: FbField[][] = [[]];
    for (const field of fields) {
      if (field.type === 'PageBreak') {
        result.push([]);
      } else if (field.type === 'HiddenField') {
        continue;
      } else {
        result[result.length - 1].push(field);
      }
    }
    return result;
  });

  pageHeadings = computed<string[]>(() => {
    const headings = [''];
    for (const field of this.body()?.fields ?? []) {
      if (field.type === 'PageBreak') headings.push(field.label ?? '');
    }
    return headings;
  });

  totalPages = computed(() => this.pages().length);
  isFirstPage = computed(() => this.currentPage() === 0);
  isLastPage = computed(() => this.currentPage() >= this.totalPages() - 1);
  currentHeading = computed(() => this.pageHeadings()[this.currentPage()] ?? '');

  visibleFieldsOnPage = computed<FbField[]>(() => {
    this.revision();
    const page = this.pages()[this.currentPage()] ?? [];
    return page.filter((f) => this.isVisible(f) && this.isRenderable(f));
  });

  // Fields on this page we recognise but cannot draw yet.
  unsupportedOnPage = computed<FbField[]>(() => {
    const page = this.pages()[this.currentPage()] ?? [];
    return page.filter((f) => this.isVisible(f) && !this.isRenderable(f));
  });

  nextPage(): void {
    if (this.isLastPage()) return;
    if (!this.validateCurrentPage()) return;
    this.currentPage.update((p) => p + 1);
  }

  previousPage(): void {
    if (this.isFirstPage()) return;
    this.currentPage.update((p) => p - 1);
  }

  private validateCurrentPage(): boolean {
    let ok = true;
    for (const field of this.visibleFieldsOnPage()) {
      const control = this.formGroup.get(controlNameFor(field));
      if (!control) continue;
      control.markAsTouched();
      if (control.invalid) ok = false;
    }
    if (!ok) this.validationFailed.emit();
    return ok;
  }

  // -- per-field helpers used by the template --------------------------------

  isRenderable(field: FbField): boolean {
    return (FB_RENDERABLE_TYPES as readonly string[]).includes(field.type);
  }

  isVisible(field: FbField): boolean {
    return isFieldVisible(field, (name) => {
      const target = (this.body()?.fields ?? []).find((f) => f.name === name);
      return target ? this.formGroup.get(controlNameFor(target))?.value : undefined;
    });
  }

  controlName(field: FbField): string {
    return controlNameFor(field);
  }

  options(field: FbField): { value: string; label: string }[] {
    return parseOptions(field);
  }

  // Rating renders as a fixed 1-5 scale, matching FormBuilder's own widget.
  ratingScale = [1, 2, 3, 4, 5];

  ratingValue(field: FbField): number {
    this.revision();
    return Number(this.formGroup.get(controlNameFor(field))?.value ?? 0);
  }

  setRating(field: FbField, value: number): void {
    this.formGroup.get(controlNameFor(field))?.setValue(value);
  }

  inputType(field: FbField): string {
    switch (field.type) {
      case 'Email': return 'email';
      case 'Number': return 'number';
      case 'Date': return 'date';
      case 'DateTime': return 'datetime-local';
      case 'Phone': return 'tel';
      case 'Password': return 'password';
      default: return 'text';
    }
  }

  errorFor(field: FbField): string | null {
    this.revision();
    const control = this.formGroup.get(controlNameFor(field));
    if (!control || !control.touched || control.valid) return null;
    return errorMessageFor(field, control.errors);
  }

  // -- submit ---------------------------------------------------------------

  async submit(): Promise<void> {
    if (this.submitting()) return;
    this.formGroup.markAllAsTouched();
    this.revision.update((n) => n + 1);

    if (this.formGroup.invalid) {
      this.validationFailed.emit();
      return;
    }

    const raw = this.formGroup.getRawValue() as Record<string, unknown>;
    const data: Record<string, string | null> = {};
    for (const field of this.body()?.fields ?? []) {
      if (field.type === 'PageBreak') continue;
      // Hidden-by-condition fields are omitted entirely, matching the server's
      // view of which answers belong to a submission.
      if (!this.isVisible(field)) continue;
      const value = raw[controlNameFor(field)];
      data[field.name] = value === undefined || value === null ? null : String(value);
    }

    const slug = this.submitToSlug();
    const base = this.apiBaseUrl();

    if (!slug) {
      this.isDone.set(true);
      this.submitted.emit({ data, posted: false });
      return;
    }

    if (!base) {
      this.submitError.emit({
        status: null,
        message: 'submitToSlug was set without apiBaseUrl, so there is nowhere to post.',
      });
      return;
    }

    this.submitting.set(true);
    try {
      const response = await fetch(
        `${base.replace(/\/$/, '')}/api/public/forms/${encodeURIComponent(slug)}/submissions`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            submitterName: (raw['submitterName'] as string | null) || null,
            submitterEmail: (raw['submitterEmail'] as string | null) || null,
            fieldValues: data,
            honeypotValue: null,
          }),
        }
      );

      if (!response.ok) {
        let message = `Submission failed (${response.status}).`;
        try {
          const problem = await response.json();
          message = problem?.message ?? problem?.detail ?? message;
        } catch {
          // Non-JSON error body — keep the status-based message.
        }
        this.submitError.emit({ status: response.status, message });
        return;
      }

      this.isDone.set(true);
      this.submitted.emit({ data, posted: true });
    } catch (err) {
      this.submitError.emit({
        status: null,
        message: err instanceof Error ? err.message : 'Network error while submitting.',
      });
    } finally {
      this.submitting.set(false);
    }
  }

  reset(): void {
    this.rebuild(this.schema());
  }
}
