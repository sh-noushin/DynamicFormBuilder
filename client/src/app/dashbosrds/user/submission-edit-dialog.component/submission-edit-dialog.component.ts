import { Component, Inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormFieldDto, FormSubmissionDto } from '../../../core/services/api-service';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidatorFn } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface SubmissionEditDialogData {
  submission: FormSubmissionDto;
  fields: FormFieldDto[];
}

interface SubmissionFieldWithKey {
  key: string;
  field: FormFieldDto;
  options: Array<{ label: string; value: string }>;
}

@Component({
  selector: 'app-submission-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatRadioModule
  ],
  templateUrl: './submission-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./submission-edit-dialog.component.scss']
})
export class SubmissionEditDialogComponent implements OnInit {
  form!: FormGroup;
  errorMessage = '';

  fieldsWithKey: SubmissionFieldWithKey[] = [];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<SubmissionEditDialogComponent, { fieldValues: { [key: string]: string } }>,
    @Inject(MAT_DIALOG_DATA) public data: SubmissionEditDialogData
  ) {}

  ngOnInit(): void {
    this.prepareFieldsWithKeys();
    this.buildForm();
  }

  private prepareFieldsWithKeys(): void {
    const usedKeys = new Set<string>();

    this.fieldsWithKey = (this.data.fields || [])
      .map(field => {
        const key = this.computeFieldKey(field);
        if (!key) {
          console.warn('[SubmissionEditDialog] skipping field without key', field);
          return null;
        }
        if (usedKeys.has(key)) {
          console.warn('[SubmissionEditDialog] duplicate key, skipping field', key, field);
          return null;
        }
        usedKeys.add(key);

        return {
          key,
          field,
          options: this.normalizedOptions(field)
        } as SubmissionFieldWithKey;
      })
      .filter((x): x is SubmissionFieldWithKey => x !== null);
  }

  private computeFieldKey(field: FormFieldDto): string | null {
    if (field.name && String(field.name).trim().length) {
      return String(field.name).trim();
    }
    if (field.id !== undefined && field.id !== null) {
      return String(field.id);
    }
    if (field.order !== undefined && field.order !== null) {
      return `field_${String(field.order)}`;
    }
    return null;
  }

  private buildForm(): void {
    const group: Record<string, any> = {};

    const valueMap = new Map<string, string>();
    const valuesArray: any[] = (this.data.submission as any).values || [];
    for (const entry of valuesArray) {
      const k = (entry.fieldName ?? '').toString();
      if (!k) continue;
      valueMap.set(k, entry.fieldValue ?? '');
    }

    for (const item of this.fieldsWithKey) {
      const { key, field } = item;

      const validators: any[] = [];
      if (field.isRequired) {
        validators.push(Validators.required);
      }
      const parsed = this.parseValidationRules(field.validation);
      try { (field as any).__validation = parsed; } catch {}

      if (parsed) {
        if (parsed.pattern) {
          try {
            const re = new RegExp(parsed.pattern);
            validators.push(Validators.pattern(re));
          } catch {
            validators.push(Validators.pattern(parsed.pattern));
          }
        }
        if (parsed.minLength != null) validators.push(Validators.minLength(Number(parsed.minLength)));
        if (parsed.maxLength != null) validators.push(Validators.maxLength(Number(parsed.maxLength)));
        if (parsed.minimum != null || parsed.maximum != null) {
          const minVal = parsed.minimum != null ? Number(parsed.minimum) : undefined;
          const maxVal = parsed.maximum != null ? Number(parsed.maximum) : undefined;
          if (this.isNumericFieldType(field.type)) {
            validators.push(this.numericRangeValidator(minVal, maxVal));
          } else {
            if (minVal !== undefined) validators.push(Validators.minLength(Math.max(0, Math.ceil(minVal))));
            if (maxVal !== undefined) validators.push(Validators.maxLength(Math.max(0, Math.ceil(maxVal))));
          }
        }
        if (parsed.allowed && Array.isArray(parsed.allowed)) validators.push(this.allowedValidator(parsed.allowed));
      }

      let rawValue: any = valueMap.get(key);
      if (rawValue === undefined || rawValue === null) {
        rawValue = field.defaultValue ?? '';
      }

      if (field.type === 'Checkbox') {
        rawValue = rawValue === 'true' || rawValue === '1';
      }

      group[key] = [rawValue, validators];
    }

    this.form = this.fb.group(group);
    try { this.form.updateValueAndValidity(); } catch {}
    try {
      for (const item of this.fieldsWithKey) {
        const ctrl = this.form.get(item.key);
        // eslint-disable-next-line no-console
        console.debug('[SubmissionEditDialog.buildForm] key=', item.key, 'validation=', item.field.validation, 'valid=', !!ctrl?.valid, 'value=', ctrl?.value);
      }
    } catch {}
  }

  private allowedValidator(allowed: any[]): ValidatorFn {
    return (control: AbstractControl) => {
      const val = control.value;
      if (val == null || val === '') return null;
      for (const a of allowed) {
        if (a === val || String(a) === String(val)) return null;
      }
      return { allowed: true };
    };
  }

  private numericRangeValidator(min?: number, max?: number): ValidatorFn {
    return (control: AbstractControl) => {
      const v = control.value;
      if (v == null || v === '') return null;

      const n = Number(v);
      if (Number.isNaN(n)) {
        return { numeric: true };
      }
      if (min !== undefined && n < min) {
        return { min: { requiredMin: min, actual: n } };
      }
      if (max !== undefined && n > max) {
        return { max: { requiredMax: max, actual: n } };
      }
      return null;
    };
  }

  private parseValidationRules(raw: unknown): any | null {
    if (raw === null || raw === undefined) return null;

    if (typeof raw === 'object') {
      if (Array.isArray(raw)) {
        return { allowed: raw };
      }
      return raw;
    }

    if (typeof raw === 'string') {
      const trimmed = raw.trim();
      if (!trimmed.length) return null;

      try {
        return JSON.parse(trimmed);
      } catch {}

      try {
        const relaxed = trimmed
          .replace(/'/g, '"')
          .replace(/,\s*}/g, '}')
          .replace(/,\s*\]/g, ']');
        return JSON.parse(relaxed);
      } catch {}

      return { pattern: trimmed };
    }

    return null;
  }

  private isNumericFieldType(type?: string | null): boolean {
    const normalized = (type ?? '').toString().trim().toLowerCase();
    return normalized === 'number' || normalized === 'integer' || normalized === 'float' || normalized === 'decimal' || normalized === 'currency';
  }

  private normalizedOptions(field: FormFieldDto): Array<{ label: string; value: string }> {
    const result: Array<{ label: string; value: string }> = [];
    let raw: any = (field as any).options;

    if (raw == null) {
      return result;
    }

    if (typeof raw === 'string') {
      const trimmed = raw.trim();
      if (!trimmed) {
        return result;
      }
      try {
        const parsed = JSON.parse(trimmed);
        raw = parsed;
      } catch {
        return trimmed
          .split(',')
          .map(s => s.trim())
          .filter(s => s.length)
          .map(s => ({ label: s, value: s }));
      }
    }

    if (Array.isArray(raw)) {
      for (const item of raw) {
        if (item && typeof item === 'object') {
          const label = String(
            (item as any).label ??
              (item as any).name ??
              (item as any).text ??
              (item as any).value ??
              '[option]'
          );
          const value = String((item as any).value ?? (item as any).id ?? label);
          result.push({ label, value });
        } else {
          const s = String(item);
          result.push({ label: s, value: s });
        }
      }

      const sequentialNumericLabels =
        result.length > 0 &&
        result.every((opt, idx) => {
          const trimmed = opt.label?.trim?.() ?? '';
          return /^\d+$/.test(trimmed) && Number(trimmed) === idx + 1;
        });

      if (sequentialNumericLabels) {
        return result.map(opt => ({ label: opt.value, value: opt.value }));
      }

      return result;
    }

    if (typeof raw === 'object') {
      for (const [k, v] of Object.entries(raw)) {
        result.push({ label: String(v), value: String(k) });
      }
    }

    return result;
  }

  getLabel(field: FormFieldDto): string {
    const l = (field.label ?? '').toString().trim();
    if (l) return l;
    const n = (field.name ?? '').toString().trim();
    if (n) return n;
    return 'Field';
  }

  // Template-safe check for whether a parsed validation object was attached to the field
  public hasParsedValidation(field: FormFieldDto | any): boolean {
    try {
      return !!(field && (field as any).__validation);
    } catch {
      return false;
    }
  }

  private shouldShowValidation(control: AbstractControl | null): boolean {
    if (!control) return false;
    const hasValue = this.controlHasValue(control);
    const interacted = control.touched || control.dirty;
    if (control.invalid && interacted) return true;
    return hasValue;
  }

  private controlHasValue(control: AbstractControl): boolean {
    const value = control.value;
    if (value === null || value === undefined) return false;
    if (typeof value === 'string') return value.trim().length > 0;
    if (typeof value === 'boolean') return value === true;
    if (Array.isArray(value)) return value.length > 0;
    return true;
  }

  public validationIconClass(control: AbstractControl | null): Record<string, boolean> {
    const show = this.shouldShowValidation(control);
    return {
      valid: !!control && control.valid && show,
      invalid: !!control && control.invalid && show,
      neutral: !show
    };
  }

  public validationIconName(control: AbstractControl | null): string {
    const show = this.shouldShowValidation(control);
    if (!show) return 'help';
    return control?.valid ? 'check_circle' : 'error';
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (!this.form) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage = 'Please fix validation errors before saving.';
      return;
    }

    const raw = this.form.getRawValue();
    const fieldValues: { [key: string]: string } = {};

    for (const item of this.fieldsWithKey) {
      const key = item.key;
      const rawValue = raw[key];

      if (item.field.type === 'Checkbox') {
        fieldValues[key] = rawValue ? 'true' : 'false';
      } else if (rawValue === null || rawValue === undefined) {
        fieldValues[key] = '';
      } else {
        fieldValues[key] = String(rawValue);
      }
    }

    this.dialogRef.close({ fieldValues });
  }
}
