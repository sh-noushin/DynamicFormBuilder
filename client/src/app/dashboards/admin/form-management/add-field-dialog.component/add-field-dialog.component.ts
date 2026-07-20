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
  // When present, the dialog opens in "edit" mode and prefills every input
  // from this field. Save emits the same payload as add; the caller decides
  // whether to POST or PUT based on whether it passed an existingField.
  existingField?: FormFieldDto;
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
  types = ['Text','Email','Number','Date','DateTime','Checkbox','Radio','Select','Textarea','Phone','Password','File','Signature','Rating','PageBreak','HiddenField'];

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
  // File-only typed constraints; serialized to/from the Validation JSON blob
  // so we don't need a schema migration for these.
  fileMaxSizeMb = signal<string>('');
  fileAllowedExts = signal<string>('');

  // Typed validation inputs for text-like and Number fields. Written into the
  // same Validation JSON alongside file constraints, so downstream backend
  // rules pick them up without any wire-format changes.
  patternRegex = signal<string>('');
  patternMessage = signal<string>('');
  minLength = signal<string>('');
  maxLength = signal<string>('');
  lengthMessage = signal<string>('');
  minimum = signal<string>('');
  maximum = signal<string>('');
  rangeMessage = signal<string>('');

  // Show If builder state - selected trigger field name + expected value.
  showIfField = signal<string>('');
  showIfEquals = signal<string>('');
  // Skip-logic target page for PageBreak fields. 1-indexed to match how the
  // admin sees pages; the client public form clamps + zero-indexes it. Blank
  // means "no skip - walk to the next page normally".
  showIfGoToPage = signal<string>('');

  optionItems = signal<Array<{label: string; value: string}>>([]);

  touched = {
    name: signal(false),
    label: signal(false),
    type: signal(false)
  };

  siblingFields: FormFieldDto[];
  isEditMode = false;

  constructor(
    private dialogRef: MatDialogRef<AddFieldDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: AddFieldDialogData | null
  ) {
    // File and Signature fields cannot be used as show-if triggers (their
    // values are opaque tokens / base64 blobs, not user-typed answers).
    const excludedTriggerTypes = new Set(['File', 'Signature']);
    // In edit mode, also exclude the field itself so it cannot depend on itself.
    const editingId = data?.existingField?.id;
    this.siblingFields = (data?.siblingFields ?? [])
      .filter(f => f.name && !excludedTriggerTypes.has(String(f.type)) && f.id !== editingId);

    if (data?.existingField) {
      this.isEditMode = true;
      this.hydrateFrom(data.existingField);
    }
  }

  private hydrateFrom(f: FormFieldDto): void {
    this.name.set(String(f.name ?? ''));
    this.label.set(String(f.label ?? ''));
    this.type.set(String(f.type ?? 'Text'));
    this.isRequired.set(!!f.isRequired);
    this.isVisible.set(f.isVisible !== false);
    this.isReadOnly.set(!!f.isReadOnly);
    this.placeholder.set(String(f.placeholder ?? ''));
    this.helpText.set(String(f.helpText ?? ''));
    this.defaultValue.set(String(f.defaultValue ?? ''));
    this.validation.set(String(f.validation ?? ''));

    // Parse the Validation JSON once and route the recognised keys into their
    // typed signals. File constraints live alongside pattern/length/range keys
    // in the same blob; the type-specific UI only exposes the relevant subset.
    if (f.validation) {
      try {
        const parsed = JSON.parse(String(f.validation));
        if (parsed && typeof parsed === 'object') {
          if (typeof parsed.maxFileSizeMb === 'number' && parsed.maxFileSizeMb > 0) {
            this.fileMaxSizeMb.set(String(parsed.maxFileSizeMb));
          }
          if (Array.isArray(parsed.allowedFileExtensions)) {
            this.fileAllowedExts.set(parsed.allowedFileExtensions.join(', '));
          }
          if (typeof parsed.pattern === 'string') this.patternRegex.set(parsed.pattern);
          if (typeof parsed.patternMessage === 'string') this.patternMessage.set(parsed.patternMessage);
          if (typeof parsed.minLength === 'number') this.minLength.set(String(parsed.minLength));
          if (typeof parsed.maxLength === 'number') this.maxLength.set(String(parsed.maxLength));
          if (typeof parsed.lengthMessage === 'string') this.lengthMessage.set(parsed.lengthMessage);
          if (typeof parsed.minimum === 'number') this.minimum.set(String(parsed.minimum));
          if (typeof parsed.maximum === 'number') this.maximum.set(String(parsed.maximum));
          if (typeof parsed.rangeMessage === 'string') this.rangeMessage.set(parsed.rangeMessage);
        }
      } catch { /* leave typed fields blank on malformed JSON */ }
    }

    if (f.options) {
      try {
        const parsed = JSON.parse(String(f.options));
        if (Array.isArray(parsed)) {
          this.optionItems.set(parsed.map((o: any) => ({
            label: String(o?.label ?? o?.value ?? ''),
            value: String(o?.value ?? '')
          })));
        }
      } catch {
        // Fallback: comma-split (legacy shape).
        this.optionItems.set(String(f.options).split(',').map(s => s.trim()).filter(Boolean).map(v => ({ label: v, value: v })));
      }
    }

    const rawShowIf = (f as any).showIfCondition as string | undefined;
    if (rawShowIf) {
      try {
        const rule = JSON.parse(rawShowIf);
        if (rule?.field) {
          this.showIfField.set(String(rule.field));
          this.showIfEquals.set(rule.equals != null ? String(rule.equals) : '');
        }
        if (typeof rule?.goToPage === 'number') {
          this.showIfGoToPage.set(String(rule.goToPage));
        }
      } catch { /* ignore invalid legacy rules */ }
    }

    // A prefilled label/name should not surface as an error before the user touches anything.
    this.touched.name.set(false);
    this.touched.label.set(false);
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

  get showFileConstraints(): boolean {
    return this.type() === 'File';
  }

  // Types where a regex/length rule makes sense. Deliberately excludes File
  // (has its own constraints), Signature/Rating/Checkbox/Radio/Select/Date/
  // DateTime/PageBreak/Number (all validated differently or not by regex).
  get showTextConstraints(): boolean {
    const t = this.type();
    return t === 'Text' || t === 'Email' || t === 'Textarea' || t === 'Password' || t === 'Phone';
  }

  get showNumberConstraints(): boolean {
    return this.type() === 'Number';
  }

  patternError(): string | null {
    const v = this.patternRegex().trim();
    if (!v) return null;
    try { new RegExp(v); return null; } catch { return 'Invalid regex'; }
  }

  private posIntOrNull(raw: string): number | null {
    const s = raw.trim();
    if (!s) return null;
    const n = Number(s);
    if (!Number.isInteger(n) || n < 0) return null;
    return n;
  }

  private numberOrNull(raw: string): number | null {
    const s = raw.trim();
    if (!s) return null;
    const n = Number(s);
    return Number.isFinite(n) ? n : null;
  }

  fileMaxSizeError(): string | null {
    const v = this.fileMaxSizeMb().trim();
    if (!v) return null;
    const n = Number(v);
    if (!Number.isInteger(n) || n < 1 || n > 1024) return 'Between 1 and 1024 MB';
    return null;
  }

  // Serializes the typed constraints into the Validation JSON blob. Emits
  // only the keys relevant to the current field type so a File field never
  // ships a stray "pattern", and a Text field never ships "maxFileSizeMb".
  private buildValidationJson(): string {
    const payload: Record<string, unknown> = {};

    if (this.showFileConstraints) {
      const maxRaw = this.fileMaxSizeMb().trim();
      if (maxRaw) {
        const n = Number(maxRaw);
        if (Number.isInteger(n) && n > 0) payload['maxFileSizeMb'] = n;
      }
      const extsRaw = this.fileAllowedExts().trim();
      if (extsRaw) {
        const list = extsRaw.split(',')
          .map(s => s.trim().replace(/^\./, '').toLowerCase())
          .filter(Boolean);
        if (list.length > 0) payload['allowedFileExtensions'] = list;
      }
    }

    if (this.showTextConstraints) {
      const pat = this.patternRegex().trim();
      if (pat) payload['pattern'] = pat;
      const patMsg = this.patternMessage().trim();
      if (patMsg) payload['patternMessage'] = patMsg;

      const min = this.posIntOrNull(this.minLength());
      if (min != null) payload['minLength'] = min;
      const max = this.posIntOrNull(this.maxLength());
      if (max != null) payload['maxLength'] = max;
      const lenMsg = this.lengthMessage().trim();
      if (lenMsg) payload['lengthMessage'] = lenMsg;
    }

    if (this.showNumberConstraints) {
      const mn = this.numberOrNull(this.minimum());
      if (mn != null) payload['minimum'] = mn;
      const mx = this.numberOrNull(this.maximum());
      if (mx != null) payload['maximum'] = mx;
      const rMsg = this.rangeMessage().trim();
      if (rMsg) payload['rangeMessage'] = rMsg;
    }

    return Object.keys(payload).length ? JSON.stringify(payload) : '';
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
    return !!(this.nameError() || this.labelError() || this.optionsInvalid() || this.fileMaxSizeError() || this.patternError());
  }

  // True when a field type has any typed constraints (File, text-like, or
  // Number). Only those field types replace the raw Validation textbox.
  get hasTypedConstraints(): boolean {
    return this.showFileConstraints || this.showTextConstraints || this.showNumberConstraints;
  }

  private buildShowIfJson(): string | undefined {
    const field = this.showIfField().trim();
    if (!field) return undefined;
    const equals = this.showIfEquals();
    // Allow an empty string equals ("show when field is blank" is a rare but
    // legitimate rule); only skip if the user did not pick a trigger.
    const payload: Record<string, unknown> = { field, equals };
    // Skip-logic target page is only meaningful on PageBreak fields; the
    // client evaluates it on Next-click to jump forward instead of walking
    // to the next page.
    if (this.type() === 'PageBreak') {
      const n = Number(this.showIfGoToPage().trim());
      if (Number.isInteger(n) && n >= 1) payload['goToPage'] = n;
    }
    return JSON.stringify(payload);
  }

  get isPageBreak(): boolean {
    return this.type() === 'PageBreak';
  }

  save() {
    this.touched.name.set(true);
    this.touched.label.set(true);
    if (this.invalid) return;
    const optionJson = this.showOptions
      ? JSON.stringify(this.optionItems().map(o => ({ label: o.label.trim(), value: o.value.trim() })))
      : '';
    // For any field type with typed constraints (File, text-like, Number),
    // the typed inputs are the source of truth and produce the Validation
    // JSON. Other types keep the raw JSON textbox as-is.
    const validationOut = this.hasTypedConstraints
      ? this.buildValidationJson()
      : this.validation();
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
      validation: validationOut,
      showIfCondition: this.buildShowIfJson(),
      options: optionJson
    };
    this.dialogRef.close(value);
  }
  cancel() { this.dialogRef.close(null); }
}
