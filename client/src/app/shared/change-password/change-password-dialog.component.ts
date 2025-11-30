import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { AbstractControl, NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatDialogContent, MatDialogActions } from "@angular/material/dialog";

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

type RequirementKey = 'length' | 'upper' | 'lower' | 'number' | 'symbol';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDialogContent,
    MatDialogActions
],
  templateUrl: './change-password-dialog.component.html',
  styleUrls: ['./change-password-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChangePasswordDialogComponent {
  private readonly fb = inject(NonNullableFormBuilder);

  @Input() pending = false;
  @Input() error: string | null = null;

  @Output() submitted = new EventEmitter<ChangePasswordPayload>();
  @Output() cancelled = new EventEmitter<void>();

  readonly hideCurrent = signal(true);
  readonly hideNew = signal(true);
  readonly hideConfirm = signal(true);

  readonly form = this.fb.group({
    currentPassword: this.fb.control('', [Validators.required]),
    newPassword: this.fb.control('', [Validators.required, Validators.minLength(8), this.passwordComplexityValidator()]),
    confirmPassword: this.fb.control('', [Validators.required])
  }, { validators: this.passwordsMatchValidator() });

  readonly passwordRequirements: readonly { key: RequirementKey; label: string }[] = [
    { key: 'length', label: 'At least 8 characters' },
    { key: 'upper', label: 'One uppercase letter' },
    { key: 'lower', label: 'One lowercase letter' },
    { key: 'number', label: 'One digit' },
    { key: 'symbol', label: 'One special character' }
  ];

  private readonly requirementChecks: Record<RequirementKey, (value: string) => boolean> = {
    length: (value) => value.length >= 8,
    upper: (value) => /[A-Z]/.test(value),
    lower: (value) => /[a-z]/.test(value),
    number: (value) => /\d/.test(value),
    symbol: (value) => /[^A-Za-z0-9]/.test(value)
  };

  get strengthScore(): number {
    const value = this.form.controls.newPassword.value;
    return this.passwordRequirements.reduce((score, requirement) => {
      return score + (this.requirementChecks[requirement.key](value) ? 1 : 0);
    }, 0);
  }

  get strengthPercentage(): number {
    return Math.round((this.strengthScore / this.passwordRequirements.length) * 100);
  }

  toggleVisibility(field: 'current' | 'new' | 'confirm') {
    switch (field) {
      case 'current':
        this.hideCurrent.update((value) => !value);
        break;
      case 'new':
        this.hideNew.update((value) => !value);
        break;
      case 'confirm':
        this.hideConfirm.update((value) => !value);
        break;
    }
  }

  requirementMet(key: RequirementKey): boolean {
    const value = this.form.controls.newPassword.value ?? '';
    return this.requirementChecks[key](value);
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { currentPassword, newPassword } = this.form.getRawValue();
    this.submitted.emit({ currentPassword, newPassword });
  }

  onCancel() {
    this.form.reset();
    this.hideCurrent.set(true);
    this.hideNew.set(true);
    this.hideConfirm.set(true);
    this.cancelled.emit();
  }

  private passwordComplexityValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value: string = control.value ?? '';
      if (!value) {
        return null;
      }

      const meetsAll = Object.values(this.requirementChecks).every((check) => check(value));
      return meetsAll ? null : { weakPassword: true };
    };
  }

  private passwordsMatchValidator(): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const password = group.get?.('newPassword');
      const confirm = group.get?.('confirmPassword');

      if (!password || !confirm) {
        return null;
      }

      const mismatch = password.value && confirm.value && password.value !== confirm.value;

      if (mismatch) {
        const existingErrors = confirm.errors ?? {};
        confirm.setErrors({ ...existingErrors, mismatch: true });
        return { mismatch: true };
      }

      if (confirm.hasError('mismatch')) {
        const { mismatch: _omit, ...rest } = confirm.errors ?? {};
        const hasOtherErrors = Object.keys(rest).length > 0;
        confirm.setErrors(hasOtherErrors ? rest : null);
      }

      return null;
    };
  }
}
