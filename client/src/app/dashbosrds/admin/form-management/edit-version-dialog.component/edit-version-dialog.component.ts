import { CommonModule } from '@angular/common';
import { Component, signal, Inject } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AddFieldDialogComponent } from '../add-field-dialog.component/add-field-dialog.component';


export type EditVersionDialogData = { description: string, formId?: string, versionNumber?: number };

@Component({
  selector: 'app-edit-version-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatIconModule, MatButtonModule, MatSelectModule, MatTableModule, MatTooltipModule],
  templateUrl: './edit-version-dialog.component.html',
  styleUrls: ['./edit-version-dialog.component.scss']
})
export class EditVersionDialogComponent {
  description = signal<string>('');
  fieldsToAdd = signal<Array<any>>([]); 
  constructor(
    private dialogRef: MatDialogRef<EditVersionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditVersionDialogData,
    private dialog: MatDialog
  ) {
    this.description.set(data.description ?? '');
  }

  submit() {
    this.dialogRef.close({
      description: this.description(),
      fields: this.fieldsToAdd()
    });
  }

  cancel() { this.dialogRef.close(null); }


  addField() {
    this.fieldsToAdd.update(arr => [...arr, { name: '', label: '', type: 'Text' }]);
  }

  removeField(index: number) {
    this.fieldsToAdd.update(arr => arr.filter((_, i) => i !== index));
  }

  openAddFieldDialog() {
    const ref = this.dialog.open(AddFieldDialogComponent, {
      width: '560px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true
    });
    ref.afterClosed().subscribe((result?: any) => {
      if (!result) return;
      this.fieldsToAdd.update(arr => [...arr, result]);
    });
  }

  setDescriptionFromEvent(ev: Event) {
    const val = (ev.target as HTMLTextAreaElement)?.value ?? '';
    this.description.set(val);
  }
  getIndex(f: { name: string; label: string; type: string }): number {
    return this.fieldsToAdd().indexOf(f);
  }
  setFieldName(f: { name: string; label: string; type: string }, ev: Event) {
    const i = this.getIndex(f); if (i < 0) return;
    const val = (ev.target as HTMLInputElement)?.value ?? '';
    this.fieldsToAdd.update(arr => arr.map((it, idx) => idx === i ? { ...it, name: val } : it));
  }
  setFieldLabel(f: { name: string; label: string; type: string }, ev: Event) {
    const i = this.getIndex(f); if (i < 0) return;
    const val = (ev.target as HTMLInputElement)?.value ?? '';
    this.fieldsToAdd.update(arr => arr.map((it, idx) => idx === i ? { ...it, label: val } : it));
  }
  setFieldType(f: { name: string; label: string; type: string }, val: string) {
    const i = this.getIndex(f); if (i < 0) return;
    this.fieldsToAdd.update(arr => arr.map((it, idx) => idx === i ? { ...it, type: val } : it));
  }
}
