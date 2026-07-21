import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

@Component({
  selector: 'app-create-tenant-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
  ],
  templateUrl: './create-tenant-dialog.component.html',
  styleUrl: './create-tenant-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class CreateTenantDialogComponent {
  name = signal('');
  provisionAdmin = signal(false);
  adminUsername = signal('');
  adminEmail = signal('');
  adminPassword = signal('');
  error = signal('');

  constructor(private dialogRef: MatDialogRef<CreateTenantDialogComponent>) {}

  save(): void {
    if (!this.name().trim()) {
      this.error.set('Tenant name is required');
      return;
    }
    if (this.provisionAdmin()) {
      if (!this.adminUsername().trim() || !this.adminEmail().trim() || !this.adminPassword().trim()) {
        this.error.set('All admin fields are required when provisioning');
        return;
      }
      if (this.adminPassword().length < 6) {
        this.error.set('Admin password must be at least 6 characters');
        return;
      }
    }
    this.dialogRef.close({
      name: this.name().trim(),
      adminUsername: this.provisionAdmin() ? this.adminUsername().trim() : null,
      adminEmail: this.provisionAdmin() ? this.adminEmail().trim() : null,
      adminPassword: this.provisionAdmin() ? this.adminPassword() : null,
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
