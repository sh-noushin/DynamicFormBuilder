import { ChangeDetectionStrategy, Component, Inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

interface RenameTenantDialogData {
  name: string;
}

@Component({
  selector: 'app-rename-tenant-dialog',
  standalone: true,
  imports: [MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  templateUrl: './rename-tenant-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class RenameTenantDialogComponent {
  name = signal('');

  constructor(
    private dialogRef: MatDialogRef<RenameTenantDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: RenameTenantDialogData,
  ) {
    this.name.set(data?.name ?? '');
  }

  save(): void {
    const trimmed = this.name().trim();
    if (trimmed) this.dialogRef.close(trimmed);
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
