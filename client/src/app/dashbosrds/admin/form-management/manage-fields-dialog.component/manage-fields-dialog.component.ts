import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Client, CreateFormFieldDto, FormFieldDto } from '../../../../core/services/api-service';
import { AddFieldDialogComponent } from '../add-field-dialog.component/add-field-dialog.component';
import { DeleteDialogComponent, DeleteDialogData } from '../../../../shared/delete-dialog.component/delete-dialog.component';

export type ManageFieldsDialogData = {
  formId: string;
  versionNumber: number;
};

@Component({
  selector: 'app-manage-fields-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatTooltipModule,
    MatSnackBarModule
  ],
  templateUrl: './manage-fields-dialog.component.html',
  styleUrls: ['./manage-fields-dialog.component.scss']
})
export class ManageFieldsDialogComponent implements OnInit {
  fields = signal<FormFieldDto[]>([]);
  displayedColumns = ['name','label','type','isRequired','order','actions'];
  isSaving = signal(false);

  constructor(
    private api: Client,
    private snack: MatSnackBar,
    private dialogRef: MatDialogRef<ManageFieldsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ManageFieldsDialogData,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadFields();
  }

  loadFields() {
    this.api.fieldsAll(this.data.formId, this.data.versionNumber).subscribe({
      next: f => this.fields.set(f),
      error: err => {
        this.snack.open('Failed to load fields', 'Close', { duration: 2500 });
      }
    });
  }

  addField() {
    if (this.isSaving()) return;
    const ref = this.dialog.open(AddFieldDialogComponent, {
      width: '560px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true
    });
    ref.afterClosed().subscribe((result?: any) => {
      if (!result) return;
      const nextOrder = this.fields().length
        ? Math.max(...this.fields().map(f => f.order ?? 0)) + 1
        : 0;
      const dto = new CreateFormFieldDto({
        name: result.name,
        label: result.label,
        type: result.type,
        order: result.order ?? nextOrder,
        isRequired: !!result.isRequired,
        isVisible: result.isVisible !== false,
        isReadOnly: !!result.isReadOnly,
        placeholder: result.placeholder || '',
        helpText: result.helpText || '',
        defaultValue: result.defaultValue || '',
        validation: result.validation || '',
        options: result.options || ''
      });
      this.isSaving.set(true);
      this.api.fieldsPOST(this.data.formId, this.data.versionNumber, dto).subscribe({
        next: _ => {
          this.snack.open('Field added', 'Close', { duration: 2000 });
          this.isSaving.set(false);
          this.loadFields();
        },
        error: err => {
          console.error('Failed to add field', err);
          this.snack.open('Failed to add field', 'Close', { duration: 3000 });
          this.isSaving.set(false);
        }
      });
    });
  }

 
  deleteField(field: FormFieldDto) {
    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        itemType: 'field',
        itemName: field.label
      } as DeleteDialogData,
      width: '400px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.api.fieldsDELETE(this.data.formId, this.data.versionNumber, field.id!).subscribe({
        next: _ => {
          this.snack.open('Field deleted', 'Close', { duration: 2000 });
          this.loadFields();
        },
        error: err => {
          this.snack.open('Failed to delete field', 'Close', { duration: 3000 });
        }
      });
    });
  }

  close() { this.dialogRef.close(); }
}
