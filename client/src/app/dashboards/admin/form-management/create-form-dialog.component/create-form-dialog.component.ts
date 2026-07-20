
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
  brandColor = signal('');
  accessPassword = signal('');
  showPassword = signal<boolean>(false);
  thankYouMessage = signal('');
  redirectUrl = signal('');
  maxSubmissions = signal('');
  closesAtInput = signal('');
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
    return !!this.nameError() || !!this.redirectUrlError() || !!this.maxSubmissionsError();
  }

  maxSubmissionsError(): string | null {
    const v = this.maxSubmissions().trim();
    if (!v) return null;
    const n = Number(v);
    if (!Number.isInteger(n) || n < 1) return 'Must be a whole number of 1 or more';
    return null;
  }

  setMaxSubmissionsFromEvent(ev: Event) {
    const val = (ev.target as HTMLInputElement)?.value ?? '';
    this.maxSubmissions.set(val);
  }
  setClosesAtFromEvent(ev: Event) {
    const val = (ev.target as HTMLInputElement)?.value ?? '';
    this.closesAtInput.set(val);
  }
  clearClosesAt() { this.closesAtInput.set(''); }

  redirectUrlError(): string | null {
    const v = this.redirectUrl().trim();
    if (!v) return null;
    try {
      const u = new URL(v);
      if (u.protocol !== 'http:' && u.protocol !== 'https:') return 'URL must start with http:// or https://';
      return null;
    } catch {
      return 'Enter a full URL (including https://)';
    }
  }

  setThankYouMessageFromEvent(ev: Event) {
    const val = (ev.target as HTMLTextAreaElement)?.value ?? '';
    this.thankYouMessage.set(val);
  }
  setRedirectUrlFromEvent(ev: Event) {
    const val = (ev.target as HTMLInputElement)?.value ?? '';
    this.redirectUrl.set(val);
  }

  submit() {
    this.touched.name.set(true);
    if (this.invalid || this.isSubmitting()) return;
    this.isSubmitting.set(true);
    const result = {
      name: this.name(),
      description: this.description(),
      brandColor: this.brandColor().trim() || undefined,
      accessPassword: this.accessPassword().trim() || undefined,
      thankYouMessage: this.thankYouMessage().trim() || undefined,
      redirectUrl: this.redirectUrl().trim() || undefined,
      maxSubmissions: this.maxSubmissions().trim() ? Number(this.maxSubmissions()) : undefined,
      closesAt: this.closesAtInput() ? new Date(this.closesAtInput()) : undefined
    };
    this.dialogRef.close(result);
  }

  clearBrandColor() { this.brandColor.set(''); }
  setAccessPasswordFromEvent(ev: Event) {
    const val = (ev.target as HTMLInputElement)?.value ?? '';
    this.accessPassword.set(val);
  }
  clearAccessPassword() { this.accessPassword.set(''); }

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

