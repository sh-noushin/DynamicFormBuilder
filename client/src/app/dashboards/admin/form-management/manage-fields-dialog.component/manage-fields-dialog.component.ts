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
import { MatMenuModule } from '@angular/material/menu';
import { Client, CreateFormFieldDto, FormFieldDto, UpdateFormFieldDto } from '../../../../core/services/api-service';
import { AddFieldDialogComponent } from '../add-field-dialog.component/add-field-dialog.component';
import { DeleteDialogComponent, DeleteDialogData } from '../../../../shared/delete-dialog.component/delete-dialog.component';
import { FIELD_TEMPLATES, FieldTemplate, uniquifyName } from '../field-templates';

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
    MatSnackBarModule,
    MatMenuModule
  ],
  templateUrl: './manage-fields-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./manage-fields-dialog.component.scss']
})
export class ManageFieldsDialogComponent implements OnInit {
  fields = signal<FormFieldDto[]>([]);
  isSaving = signal(false);
  reordering = signal(false);
  templates: FieldTemplate[] = FIELD_TEMPLATES;

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
      disableClose: true,
      data: { siblingFields: this.fields() }
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

  editField(field: FormFieldDto) {
    if (this.isSaving() || !field.id) return;
    const ref = this.dialog.open(AddFieldDialogComponent, {
      width: '560px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true,
      data: { siblingFields: this.fields(), existingField: field }
    });
    ref.afterClosed().subscribe((result?: any) => {
      if (!result || !field.id) return;
      const dto = new UpdateFormFieldDto({
        formVersionId: field.formVersionId,
        name: result.name,
        label: result.label,
        type: result.type,
        order: field.order ?? 0,
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
      this.api.fieldsPUT(this.data.formId, this.data.versionNumber, field.id, dto).subscribe({
        next: () => {
          this.snack.open('Field updated', 'Close', { duration: 2000 });
          this.isSaving.set(false);
          this.loadFields();
        },
        error: () => {
          this.snack.open('Failed to update field', 'Close', { duration: 3000 });
          this.isSaving.set(false);
        }
      });
    });
  }

  duplicateField(field: FormFieldDto) {
    if (this.isSaving() || !field.id) return;
    const existing = new Set(this.fields().map(f => String(f.name ?? '').toLowerCase()));
    const newName = uniquifyName(String(field.name ?? 'field'), existing);
    this.isSaving.set(true);
    const dto = new CreateFormFieldDto({
      name: newName,
      label: `${field.label} (copy)`,
      type: String(field.type),
      // Temporary order — the reorder call below places the clone right
      // after its source so admins don't have to drag it into position.
      order: this.fields().length + 1,
      isRequired: !!field.isRequired,
      isVisible: field.isVisible !== false,
      isReadOnly: !!field.isReadOnly,
      placeholder: field.placeholder || '',
      helpText: field.helpText || '',
      defaultValue: field.defaultValue || '',
      validation: field.validation || '',
      options: field.options || '',
      showIfCondition: (field as any).showIfCondition || undefined,
    });
    this.api.fieldsPOST(this.data.formId, this.data.versionNumber, dto).subscribe({
      next: created => {
        // Re-fetch so we're reordering against the freshest server state,
        // then shove the new row into the slot right after its source.
        this.api.fieldsAll(this.data.formId, this.data.versionNumber).subscribe({
          next: fresh => {
            const sorted = fresh.slice().sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
            const withoutNew = sorted.filter(f => f.id !== created.id);
            const sourceIdx = withoutNew.findIndex(f => f.id === field.id);
            const insertIdx = sourceIdx >= 0 ? sourceIdx + 1 : withoutNew.length;
            const finalOrder = [...withoutNew.slice(0, insertIdx), created, ...withoutNew.slice(insertIdx)];
            const ids = finalOrder.map(f => f.id!).filter(Boolean);
            this.api.reorder(this.data.formId, this.data.versionNumber, ids).subscribe({
              next: () => {
                this.isSaving.set(false);
                this.snack.open('Field duplicated', 'Close', { duration: 2000 });
                this.loadFields();
              },
              error: () => {
                // Order didn't stick, but the clone itself exists - keep
                // the reload so the admin at least sees the new field.
                this.isSaving.set(false);
                this.snack.open('Field duplicated; order not saved', 'Close', { duration: 3000 });
                this.loadFields();
              },
            });
          },
          error: () => {
            this.isSaving.set(false);
            this.snack.open('Field duplicated', 'Close', { duration: 2000 });
            this.loadFields();
          },
        });
      },
      error: () => {
        this.isSaving.set(false);
        this.snack.open('Failed to duplicate field', 'Close', { duration: 3000 });
      },
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

  // Applies a curated template by POSTing each of its fields sequentially.
  // Sequential (not parallel) preserves the intended order because Order is
  // assigned from the current field-count baseline. Name collisions are
  // resolved client-side via suffix so a duplicate name never breaks a batch
  // half-way through.
  applyTemplate(template: FieldTemplate) {
    if (this.isSaving()) return;
    const existing = new Set(this.fields().map(f => String(f.name ?? '').toLowerCase()));
    const baseOrder = this.fields().length;
    const queue = template.fields.map((tf, i) => new CreateFormFieldDto({
      name: uniquifyName(tf.name, existing),
      label: tf.label,
      type: tf.type,
      order: baseOrder + i + 1,
      isRequired: !!tf.isRequired,
      isVisible: true,
      isReadOnly: false,
      placeholder: tf.placeholder || '',
      helpText: tf.helpText || '',
      defaultValue: '',
      validation: tf.validation || '',
      options: tf.options || '',
    }));

    this.isSaving.set(true);
    let failures = 0;
    const runNext = (index: number) => {
      if (index >= queue.length) {
        this.isSaving.set(false);
        this.loadFields();
        const added = queue.length - failures;
        const msg = failures === 0
          ? `Added ${added} field${added === 1 ? '' : 's'} from "${template.name}"`
          : `Added ${added} of ${queue.length} fields from "${template.name}"`;
        this.snack.open(msg, 'Close', { duration: 2500 });
        return;
      }
      this.api.fieldsPOST(this.data.formId, this.data.versionNumber, queue[index]).subscribe({
        next: () => runNext(index + 1),
        error: () => { failures++; runNext(index + 1); },
      });
    };
    runNext(0);
  }
}
