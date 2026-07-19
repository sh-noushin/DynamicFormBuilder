
import { Component, Inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AddFieldDialogComponent } from '../add-field-dialog.component/add-field-dialog.component';

export type CreateVersionDialogData = { nextVersionNumber?: number };

@Component({
  selector: 'app-create-version-dialog',
  standalone: true,
  imports: [MatDialogModule, MatFormFieldModule, MatInputModule, MatIconModule, MatButtonModule, MatSelectModule, MatTableModule, MatSlideToggleModule, MatTooltipModule],
  templateUrl: './create-version-dialog.component.html',
  styleUrls: ['./create-version-dialog.component.scss']
})
export class CreateVersionDialogComponent {
  description = signal('');
  publish = signal(false);
  makeCurrent = signal(false);
  initialFields = signal<Array<any>>([]);

  constructor(
    private dialogRef: MatDialogRef<CreateVersionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CreateVersionDialogData,
    private dialog: MatDialog
  ) {}

  submit() {
    this.dialogRef.close({
      description: this.description(),
      publish: this.publish(),
      makeCurrent: this.makeCurrent(),
      fields: this.initialFields()
    });
  }

  cancel() { this.dialogRef.close(null); }

  addInitialField() {
    this.initialFields.update(fields => [...fields, { name: '', label: '', type: 'Text' }]);
  }

  removeInitialField(index: number) {
    this.initialFields.update(fields => fields.filter((_, i) => i !== index));
  }

  openAddFieldDialog() {
    const ref = this.dialog.open(AddFieldDialogComponent, {
      width: '560px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true
    });
    ref.afterClosed().subscribe((result?: any) => {
      if (!result) return;
      this.initialFields.update(fields => [...fields, result]);
    });
  }

  setFieldName(index: number, ev: Event) {
    const value = (ev.target as HTMLInputElement)?.value ?? '';
    this.initialFields.update(fs => {
      const copy = [...fs];
      copy[index] = { ...copy[index], name: value };
      return copy;
    });
  }

  setFieldLabel(index: number, ev: Event) {
    const value = (ev.target as HTMLInputElement)?.value ?? '';
    this.initialFields.update(fs => {
      const copy = [...fs];
      copy[index] = { ...copy[index], label: value };
      return copy;
    });
  }

  setFieldType(index: number, value: string) {
    this.initialFields.update(fs => {
      const copy = [...fs];
      copy[index] = { ...copy[index], type: value };
      return copy;
    });
  }

  getIndex(f: { name: string; label: string; type: string }): number {
    return this.initialFields().indexOf(f);
  }
}
