
import { Component, signal, Inject, ChangeDetectionStrategy } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';


export type EditVersionDialogData = { description: string, formId?: string, versionNumber?: number };

@Component({
  selector: 'app-edit-version-dialog',
  standalone: true,
  imports: [MatDialogModule, MatFormFieldModule, MatInputModule, MatIconModule, MatButtonModule],
  templateUrl: './edit-version-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./edit-version-dialog.component.scss']
})
export class EditVersionDialogComponent {
  description = signal<string>('');

  constructor(
    private dialogRef: MatDialogRef<EditVersionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditVersionDialogData,
    private router: Router,
    private matDialog: MatDialog
  ) {
    this.description.set(data.description ?? '');
  }

  submit() {
    // fields is always empty now — field editing lives in the drag-and-drop
    // builder. Kept in the payload so the caller in edit-form-dialog keeps
    // working without changes.
    this.dialogRef.close({
      description: this.description(),
      fields: []
    });
  }

  cancel() { this.dialogRef.close(null); }

  openBuilder() {
    if (!this.data.formId || this.data.versionNumber == null) return;
    // Close every open dialog first — the parent Edit Form dialog would
    // otherwise stay mounted with its backdrop, covering the builder page.
    this.matDialog.closeAll();
    this.router.navigate(['/admin/forms', this.data.formId, 'versions', this.data.versionNumber, 'builder']);
  }

  setDescriptionFromEvent(ev: Event) {
    const val = (ev.target as HTMLTextAreaElement)?.value ?? '';
    this.description.set(val);
  }
}
