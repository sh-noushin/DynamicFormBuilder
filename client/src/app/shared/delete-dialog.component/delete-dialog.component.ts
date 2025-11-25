import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

export interface DeleteDialogData {
  itemType: string; // e.g. 'form', 'user', 'version'
  itemName?: string; // e.g. 'Form 1', 'Nooshin', 'v2'
}

@Component({
  selector: 'app-delete-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  templateUrl: './delete-dialog.component.html',
  styleUrls: ['./delete-dialog.component.css']
})
export class DeleteDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<DeleteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DeleteDialogData
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onDelete(): void {
    this.dialogRef.close(true);
  }
}
