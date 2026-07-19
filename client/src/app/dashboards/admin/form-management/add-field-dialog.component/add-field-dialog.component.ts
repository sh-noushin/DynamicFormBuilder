
import { Component, signal, ChangeDetectionStrategy } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-add-field-dialog',
  standalone: true,
  imports: [MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatCheckboxModule],
  templateUrl: './add-field-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./add-field-dialog.component.scss']
})
export class AddFieldDialogComponent {
  types = ['Text','Email','Number','Date','DateTime','Checkbox','Radio','Select','Textarea','Phone','Password','File'];

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
  showIfCondition = signal('');

  optionItems = signal<Array<{label: string; value: string}>>([]);

  touched = {
    name: signal(false),
    label: signal(false),
    type: signal(false)
  };

  constructor(private dialogRef: MatDialogRef<AddFieldDialogComponent>) {}

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
      showIfCondition: this.showIfCondition().trim() || undefined,
      options: optionJson
    };
    this.dialogRef.close(value);
  }
  cancel() { this.dialogRef.close(null); }
}
