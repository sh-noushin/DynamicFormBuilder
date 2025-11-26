import { Component, Inject, signal } from '@angular/core';
import { Client, RegisterUserDto, UpdateUserDto, UserDto, UserRole } from '../../../../core/services/api-service';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export type UserDialogMode = 'create' | 'edit';

export interface UserDialogData {
  mode: UserDialogMode;
  user?: UserDto | null;
}

@Component({
  selector: 'app-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './user-dialog.component.html',
  styleUrls: ['./user-dialog.component.scss']
})
export class UserDialogComponent {
  UserRole = UserRole;
  roles = Object.values(UserRole);

  submitting = signal(false);
  error = signal<string | null>(null);


  username = signal('');
  email = signal('');
  role = signal<UserRole>(UserRole.User);
  password = signal('');

  touched = {
    username: signal(false),
    email: signal(false),
    role: signal(false),
    password: signal(false)
  };


  usernameError = () => {
    if (!this.touched.username()) return null;
    if (!this.username().trim()) return 'Username is required';
    if (this.username().length < 3) return 'Minimum 3 characters';
    return null;
  };
  emailError = () => {
    if (!this.touched.email()) return null;
    if (!this.email().trim()) return 'Email is required';
    if (!/^\S+@\S+\.\S+$/.test(this.email())) return 'Enter a valid email';
    return null;
  };
  roleError = () => {
    if (!this.touched.role()) return null;
    if (!this.role()) return 'Role is required';
    return null;
  };
  passwordError = () => {
    if (!this.isCreate) return null;
    if (!this.touched.password()) return null;
    if (!this.password().trim()) return 'Password is required';
    if (this.password().length < 6) return 'Minimum 6 characters';
    return null;
  };

  get invalid(): boolean {
    return !!(
      this.usernameError() ||
      this.emailError() ||
      this.roleError() ||
      (this.isCreate && this.passwordError())
    );
  }
  constructor(
    private api: Client,
    private dialogRef: MatDialogRef<UserDialogComponent, UserDto | null>,
    @Inject(MAT_DIALOG_DATA) public data: UserDialogData
  ) {
    if (data?.mode === 'edit' && data.user) {
      this.username.set(data.user.username ?? '');
      this.email.set(data.user.email ?? '');
      this.role.set((data.user.roles && data.user.roles.length ? data.user.roles[0] as unknown as UserRole : UserRole.User));
    }
  }

  get title(): string {
    return this.data?.mode === 'edit' ? 'Edit User' : 'Create User';
  }

  get isCreate(): boolean { return this.data?.mode !== 'edit'; }

  submit() {
    Object.values(this.touched).forEach(s => s.set(true));
    if (this.invalid) return;

    this.submitting.set(true);
    this.error.set(null);

    if (this.isCreate) {
      const payload = new RegisterUserDto({
        username: this.username() ?? undefined,
        email: this.email() ?? undefined,
        password: this.password() ?? undefined,
        role: this.role() ?? undefined,
      });
      this.api.register(payload).subscribe({
        next: (created: UserDto) => {
          this.submitting.set(false);
          this.dialogRef.close(created);
        },
        error: (err: unknown) => {
          console.error('Create user failed', err);
          this.submitting.set(false);
          this.error.set('Failed to create user');
        }
      });
    } else if (this.data?.user?.id) {
      const payload = new UpdateUserDto({
        username: this.username() ?? undefined,
        email: this.email() ?? undefined,
        role: this.role() as unknown as number ?? undefined,
      });
      this.api.userPUT(this.data.user.id, payload).subscribe({
        next: (updated: UserDto) => {
          this.submitting.set(false);
          this.dialogRef.close(updated);
        },
        error: (err: any) => {
          console.error('Update user failed', err);
          this.submitting.set(false);
          let message = 'Failed to update user';
          if (err?.error?.Message) message = err.error.Message;
          else if (err?.error?.message) message = err.error.message;
          this.error.set(message);
        }
      });
    } else {
      this.submitting.set(false);
      this.error.set('Missing user id for update');
    }
  }

  cancel() {
    this.dialogRef.close(null);
  }
}
