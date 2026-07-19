
import { Component, Inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

export type CreateFormDialogData = {
  name?: string;
  description?: string;
};

@Component({
  selector: 'app-create-form-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule
],
  templateUrl: './create-form-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./create-form-dialog.component.scss']
})
export class CreateFormDialogComponent {
  isSubmitting = signal(false);
  name = signal('');
  description = signal('');
  touched = {
    name: signal(false),
    description: signal(false)
  };

  constructor(
    private dialogRef: MatDialogRef<CreateFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CreateFormDialogData
  ) {
    if (data?.name) this.name.set(data.name);
    if (data?.description) this.description.set(data.description);
  }

  nameError = () => {
    if (!this.touched.name()) return null;
    if (!this.name().trim()) return 'Name is required';
    if (this.name().length < 3) return 'Minimum 3 characters';
    return null;
  };

  get invalid(): boolean {
    return !!this.nameError();
  }

  submit() {
    this.touched.name.set(true);
    if (this.invalid || this.isSubmitting()) return;
    this.isSubmitting.set(true);
    const result = {
      name: this.name(),
      description: this.description()
    };
    this.dialogRef.close(result);
  }

  cancel() {
    this.dialogRef.close(null);
  }

  setNameFromEvent(ev: Event) {
    const val = (ev.target as HTMLInputElement)?.value ?? '';
    this.name.set(val);
  }
  setDescriptionFromEvent(ev: Event) {
    const val = (ev.target as HTMLTextAreaElement)?.value ?? '';
    this.description.set(val);
  }
}

