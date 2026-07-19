import { Component, Inject, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
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
    CdkDropList,
    CdkDrag,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatSnackBarModule
  ],
  templateUrl: './manage-fields-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./manage-fields-dialog.component.scss']
})
export class ManageFieldsDialogComponent implements OnInit {
  fields = signal<FormFieldDto[]>([]);
  isSaving = signal(false);
  reordering = signal(false);

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
      next: fields => {
        const sorted = [...fields].sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
        this.fields.set(sorted);
      },
      error: () => this.snack.open('Failed to load fields', 'Close', { duration: 2500 })
    });
  }

  onDrop(event: CdkDragDrop<FormFieldDto[]>) {
    if (event.previousIndex === event.currentIndex) return;
    const list = [...this.fields()];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    this.fields.set(list);
    this.persistOrder(list);
  }

  private persistOrder(list: FormFieldDto[]) {
    const ids = list.map(f => f.id!).filter(Boolean);
    this.reordering.set(true);
    this.api.reorder(this.data.formId, this.data.versionNumber, ids).subscribe({
      next: () => {
        this.reordering.set(false);
      },
      error: () => {
        this.reordering.set(false);
        this.snack.open('Failed to save new order', 'Close', { duration: 3000 });
        this.loadFields();
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
      const nextOrder = this.fields().length + 1;
      const dto = new CreateFormFieldDto({
        name: result.name,
        label: result.label,
        type: result.type,
        order: nextOrder,
        isRequired: !!result.isRequired,
        isVisible: result.isVisible !== false,
        isReadOnly: !!result.isReadOnly,
        placeholder: result.placeholder || '',
        helpText: result.helpText || '',
        defaultValue: result.defaultValue || '',
        validation: result.validation || '',
        options: result.options || '',
        showIfCondition: result.showIfCondition || undefined
      });
      this.isSaving.set(true);
      this.api.fieldsPOST(this.data.formId, this.data.versionNumber, dto).subscribe({
        next: () => {
          this.snack.open('Field added', 'Close', { duration: 2000 });
          this.isSaving.set(false);
          this.loadFields();
        },
        error: () => {
          this.snack.open('Failed to add field', 'Close', { duration: 3000 });
          this.isSaving.set(false);
        }
      });
    });
  }

  deleteField(field: FormFieldDto) {
    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: { itemType: 'field', itemName: field.label } as DeleteDialogData,
      width: '400px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed || !field.id) return;
      this.api.fieldsDELETE(this.data.formId, this.data.versionNumber, field.id).subscribe({
        next: () => {
          this.snack.open('Field deleted', 'Close', { duration: 2000 });
          this.loadFields();
        },
        error: () => this.snack.open('Failed to delete field', 'Close', { duration: 3000 })
      });
    });
  }

  close() { this.dialogRef.close(); }
}
