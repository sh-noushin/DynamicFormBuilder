import { Component, Inject, signal, ChangeDetectionStrategy, computed } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FormFieldDto } from '../../../../core/services/api-service';

export interface AddFieldDialogData {
  siblingFields?: FormFieldDto[];
}

@Component({
  selector: 'app-add-field-dialog',
  standalone: true,
  imports: [MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatCheckboxModule],
  templateUrl: './add-field-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./add-field-dialog.component.scss']
})
export class AddFieldDialogComponent {
  types = ['Text','Email','Number','Date','DateTime','Checkbox','Radio','Select','Textarea','Phone','Password','File','Signature','Rating'];

  name = signal('');
  label = signal('');
  type = signal('Text');
  isRequired = signal(false);
  isVisible = signal(true);
  isReadOnly = signal(false);
  placeholder = signal('');
  helpText = signal('');
  defaultValue = signal('');
  validation = signal('');

  // Show If builder state - selected trigger field name + expected value.
  showIfField = signal<string>('');
  showIfEquals = signal<string>('');

  optionItems = signal<Array<{label: string; value: string}>>([]);

  touched = {
    name: signal(false),
    label: signal(false),
    type: signal(false)
  };

  siblingFields: FormFieldDto[];

  constructor(
    private dialogRef: MatDialogRef<AddFieldDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: AddFieldDialogData | null
  ) {
    // File and Signature fields cannot be used as show-if triggers (their
    // values are opaque tokens / base64 blobs, not user-typed answers).
    const excludedTriggerTypes = new Set(['File', 'Signature']);
    this.siblingFields = (data?.siblingFields ?? [])
      .filter(f => f.name && !excludedTriggerTypes.has(String(f.type)));
  }

  // Options of the currently selected trigger field, if any (Select or Radio).
  triggerFieldOptions = computed<Array<{ value: string; label: string }>>(() => {
    const target = this.siblingFields.find(f => f.name === this.showIfField());
    if (!target) return [];
    const raw = target.options as string | undefined;
    if (!raw) return [];
    try {
      const parsed = JSON.parse(raw);
      if (Array.isArray(parsed)) {
        return parsed
          .map((o: any) => ({ value: String(o?.value ?? ''), label: String(o?.label ?? o?.value ?? '') }))
          .filter(o => o.value.length);
      }
    } catch { /* fall through to comma-split */ }
    return raw.split(',').map(s => s.trim()).filter(Boolean).map(s => ({ value: s, label: s }));
  });

  triggerFieldType = computed<string>(() => {
    const target = this.siblingFields.find(f => f.name === this.showIfField());
    return target ? String(target.type ?? '') : '';
  });

  nameError = () => {
    if (!this.touched.name()) return null;
    if (!this.name().trim()) return 'Name is required';
    if (!/^[a-zA-Z_][a-zA-Z0-9_]*$/.test(this.name())) return 'Use letters, numbers, and underscores; start with a letter or underscore';
    if (this.name().length > 64) return 'Max 64 characters';
    return null;
  };
  labelError = () => {
    if (!this.touched.label()) return null;
    if (!this.label().trim()) return 'Label is required';
    if (this.label().length > 100) return 'Max 100 characters';
    return null;
  };

  get showOptions(): boolean {
    return this.type() === 'Select' || this.type() === 'Radio';
  }

  private ensureInitialOptionRow() {
    if (this.showOptions && this.optionItems().length === 0) {
      this.optionItems.set([{ label: '', value: '' }]);
    }
    if (!this.showOptions && this.optionItems().length) {
      this.optionItems.set([]);
    }
  }

  typeChanged(newType: string) {
    this.type.set(newType);
    this.touched.type.set(true);
    this.ensureInitialOptionRow();
  }

  onShowIfFieldChange(fieldName: string) {
    this.showIfField.set(fieldName);
    // Reset equals when trigger changes so a stale value cannot linger.
    this.showIfEquals.set('');
  }

  addOption() {
    this.optionItems.set([...this.optionItems(), { label: '', value: '' }]);
  }
  removeOption(index: number) {
    const arr = [...this.optionItems()];
    arr.splice(index, 1);
    this.optionItems.set(arr);
  }
  updateOptionLabel(index: number, value: string) {
    const arr = [...this.optionItems()];
    arr[index] = { ...arr[index], label: value };
    this.optionItems.set(arr);
  }
  updateOptionValue(index: number, value: string) {
    const arr = [...this.optionItems()];
    arr[index] = { ...arr[index], value: value };
    this.optionItems.set(arr);
  }

  optionsInvalid(): boolean {
    if (!this.showOptions) return false;
    const items = this.optionItems();
    if (items.length === 0) return true;
    return items.some(o => !o.label.trim() || !o.value.trim());
  }

  get invalid(): boolean {
    return !!(this.nameError() || this.labelError() || this.optionsInvalid());
  }

  private buildShowIfJson(): string | undefined {
    const field = this.showIfField().trim();
    if (!field) return undefined;
    const equals = this.showIfEquals();
    // Allow an empty string equals ("show when field is blank" is a rare but
    // legitimate rule); only skip if the user did not pick a trigger.
    return JSON.stringify({ field, equals });
  }

  save() {
    this.touched.name.set(true);
    this.touched.label.set(true);
    if (this.invalid) return;
    const optionJson = this.showOptions
      ? JSON.stringify(this.optionItems().map(o => ({ label: o.label.trim(), value: o.value.trim() })))
      : '';
    const value = {
      name: this.name(),
      label: this.label(),
      type: this.type(),
      isRequired: this.isRequired(),
      isVisible: this.isVisible(),
      isReadOnly: this.isReadOnly(),
      placeholder: this.placeholder(),
      helpText: this.helpText(),
      defaultValue: this.defaultValue(),
      validation: this.validation(),
      showIfCondition: this.buildShowIfJson(),
      options: optionJson
    };
    this.dialogRef.close(value);
  }
  cancel() { this.dialogRef.close(null); }
}
