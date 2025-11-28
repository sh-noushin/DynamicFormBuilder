import { Component, Inject, OnInit } from '@angular/core';
import { FormFieldDto, FormSubmissionDto } from '../../../core/services/api-service';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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
      if (field.validation) {
        try {
          validators.push(Validators.pattern(field.validation));
        } catch {
          
        }
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
