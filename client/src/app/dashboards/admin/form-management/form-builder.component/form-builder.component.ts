import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Client, CreateFormFieldDto, FormFieldDto, UpdateFormFieldDto } from '../../../../core/services/api-service';
import { FIELD_TYPE_CATALOG, FieldTypeEntry } from './field-type-catalog';

// A field as it lives in the builder canvas. `id === null` means it is a
// new draft that has not been persisted to the API yet; save() flushes
// those via fieldsPOST. `dirty` marks fields whose properties have been
// edited since load. `deleted` tombstones fields the admin removed on the
// canvas but that still exist on the server.
interface DraftField {
  clientId: string;         // stable within this session
  id: string | null;        // server id when saved
  name: string;
  label: string;
  type: string;
  order: number;
  isRequired: boolean;
  isVisible: boolean;
  isReadOnly: boolean;
  placeholder: string;
  helpText: string;
  defaultValue: string;
  validation: string;       // JSON blob
  options: string;          // JSON blob for Radio/Select
  showIfCondition?: string; // JSON
  dirty: boolean;
  deleted: boolean;
}

let nextClientId = 1;

@Component({
  selector: 'app-form-builder',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [
    CommonModule,
    RouterModule,
    CdkDropList,
    CdkDrag,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatTooltipModule,
    MatSlideToggleModule,
    MatSnackBarModule,
  ],
  templateUrl: './form-builder.component.html',
  styleUrl: './form-builder.component.scss',
})
export class FormBuilderComponent {
  private route = inject(ActivatedRoute);
  private api = inject(Client);
  private snack = inject(MatSnackBar);

  toolbox = FIELD_TYPE_CATALOG;

  formId = signal<string>('');
  versionNumber = signal<number>(0);
  loading = signal(true);
  saving = signal(false);
  fields = signal<DraftField[]>([]);
  selectedClientId = signal<string | null>(null);

  visibleFields = computed(() => this.fields().filter(f => !f.deleted));
  selectedField = computed<DraftField | null>(() => {
    const id = this.selectedClientId();
    if (!id) return null;
    return this.fields().find(f => f.clientId === id) ?? null;
  });
  hasChanges = computed(() =>
    this.fields().some(f => f.dirty || f.deleted || f.id === null)
  );

  // Cached tag string used by the cdkDropList[connectedTo] binding so the
  // toolbox can drop items onto the canvas.
  readonly CANVAS_LIST_ID = 'builder-canvas-list';
  readonly TOOLBOX_LIST_ID = 'builder-toolbox-list';

  constructor() {
    const formId = this.route.snapshot.paramMap.get('id') ?? '';
    const version = Number(this.route.snapshot.paramMap.get('version') ?? '1');
    this.formId.set(formId);
    this.versionNumber.set(version);
    this.loadFields();
  }

  private loadFields(): void {
    if (!this.formId() || !this.versionNumber()) {
      this.loading.set(false);
      return;
    }
    this.api.fieldsAll(this.formId(), this.versionNumber()).subscribe({
      next: (rows) => {
        const drafts: DraftField[] = (rows ?? [])
          .slice()
          .sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
          .map((f) => ({
            clientId: `srv-${f.id}-${nextClientId++}`,
            id: String(f.id ?? ''),
            name: String(f.name ?? ''),
            label: String(f.label ?? ''),
            type: String(f.type ?? 'Text'),
            order: Number(f.order ?? 0),
            isRequired: !!f.isRequired,
            isVisible: f.isVisible !== false,
            isReadOnly: !!f.isReadOnly,
            placeholder: String(f.placeholder ?? ''),
            helpText: String(f.helpText ?? ''),
            defaultValue: String(f.defaultValue ?? ''),
            validation: String(f.validation ?? ''),
            options: String(f.options ?? ''),
            showIfCondition: (f as any).showIfCondition ?? undefined,
            dirty: false,
            deleted: false,
          }));
        this.fields.set(drafts);
        this.loading.set(false);
      },
      error: () => {
        this.snack.open('Failed to load fields', 'Close', { duration: 3000 });
        this.loading.set(false);
      },
    });
  }

  // Drop handler: fires both for "reorder inside canvas" and "drag from
  // toolbox onto canvas". Same event shape, different source-container id.
  onCanvasDrop(event: CdkDragDrop<any>): void {
    if (event.previousContainer.id === this.CANVAS_LIST_ID) {
      // Internal reorder.
      const list = this.visibleFields();
      moveItemInArray(list, event.previousIndex, event.currentIndex);
      // Rewrite orders for all visible fields.
      const reordered = this.fields().map((f) => {
        const idx = list.findIndex(v => v.clientId === f.clientId);
        return idx >= 0 ? { ...f, order: idx + 1, dirty: true } : f;
      });
      this.fields.set(reordered);
      return;
    }
    // From toolbox: previousContainer.data[previousIndex] is the FieldTypeEntry.
    const template = (event.previousContainer.data as FieldTypeEntry[])?.[event.previousIndex];
    if (!template) return;
    const draft = this.newDraftFromTemplate(template);
    const before = this.visibleFields();
    const insertIdx = Math.max(0, Math.min(event.currentIndex, before.length));
    const newVisible = [...before.slice(0, insertIdx), draft, ...before.slice(insertIdx)];
    // Merge deleted fields back so they still ship in dirty state.
    const deleted = this.fields().filter(f => f.deleted);
    const withOrder = newVisible.map((f, i) => ({ ...f, order: i + 1, dirty: f.dirty || f.clientId === draft.clientId }));
    this.fields.set([...withOrder, ...deleted]);
    this.selectedClientId.set(draft.clientId);
  }

  private newDraftFromTemplate(t: FieldTypeEntry): DraftField {
    // Derive a snake_case field name from the type + a running counter so
    // two "Short text" drops don't collide on the required unique-name.
    const existing = new Set(this.fields().map(f => f.name.toLowerCase()));
    const base = t.type.toLowerCase().replace(/[^a-z0-9_]/g, '_');
    let n = 1;
    let candidate = base;
    while (existing.has(candidate)) {
      n += 1;
      candidate = `${base}_${n}`;
    }
    return {
      clientId: `new-${nextClientId++}`,
      id: null,
      name: candidate,
      label: t.defaultLabel,
      type: t.type,
      order: this.visibleFields().length + 1,
      isRequired: false,
      isVisible: true,
      isReadOnly: false,
      placeholder: '',
      helpText: '',
      defaultValue: '',
      validation: '',
      options: t.type === 'Radio' || t.type === 'Select'
        ? '[{"value":"option_1","label":"Option 1"},{"value":"option_2","label":"Option 2"}]'
        : '',
      dirty: true,
      deleted: false,
    };
  }

  selectField(id: string): void {
    this.selectedClientId.set(id);
  }

  deleteSelected(): void {
    const id = this.selectedClientId();
    if (!id) return;
    this.fields.set(this.fields().map(f => f.clientId === id ? { ...f, deleted: true, dirty: true } : f));
    this.selectedClientId.set(null);
  }

  updateSelected<K extends keyof DraftField>(key: K, value: DraftField[K]): void {
    const id = this.selectedClientId();
    if (!id) return;
    this.fields.set(this.fields().map(f =>
      f.clientId === id ? { ...f, [key]: value, dirty: true } : f
    ));
  }

  save(): void {
    if (this.saving() || !this.formId() || !this.versionNumber()) return;
    this.saving.set(true);

    // Compute the ops we need to run:
    //   - deletes: existing server fields marked deleted
    //   - creates: fields with id === null
    //   - updates: fields with an id that are dirty
    // Order rewrite happens after creates so all fields have server ids.
    const all = this.fields();
    const deletes = all.filter(f => f.deleted && f.id);
    const creates = all.filter(f => !f.deleted && f.id === null);
    const updates = all.filter(f => !f.deleted && f.id && f.dirty);

    const runSequential = async () => {
      try {
        for (const f of deletes) {
          await this.api.fieldsDELETE(this.formId(), this.versionNumber(), f.id!).toPromise();
        }
        for (const f of updates) {
          const dto = new UpdateFormFieldDto({
            formVersionId: this.formIdToVersionGuid(),
            name: f.name,
            label: f.label,
            type: f.type,
            order: f.order,
            isRequired: f.isRequired,
            isVisible: f.isVisible,
            isReadOnly: f.isReadOnly,
            placeholder: f.placeholder,
            helpText: f.helpText,
            defaultValue: f.defaultValue,
            validation: f.validation,
            options: f.options,
            showIfCondition: f.showIfCondition,
          });
          await this.api.fieldsPUT(this.formId(), this.versionNumber(), f.id!, dto).toPromise();
        }
        for (const f of creates) {
          const dto = new CreateFormFieldDto({
            name: f.name,
            label: f.label,
            type: f.type,
            order: f.order,
            isRequired: f.isRequired,
            isVisible: f.isVisible,
            isReadOnly: f.isReadOnly,
            placeholder: f.placeholder,
            helpText: f.helpText,
            defaultValue: f.defaultValue,
            validation: f.validation,
            options: f.options,
            showIfCondition: f.showIfCondition,
          });
          const created = await this.api.fieldsPOST(this.formId(), this.versionNumber(), dto).toPromise();
          if (created?.id) {
            // Attach the server id to our draft so a follow-up save doesn't
            // try to re-create it.
            f.id = String(created.id);
            f.dirty = false;
          }
        }
        // Reorder in one call so the server-side sequence matches the canvas.
        const orderedVisible = this.visibleFields();
        const ids = orderedVisible.map(f => f.id).filter((id): id is string => !!id);
        if (ids.length > 0) {
          await this.api.reorder(this.formId(), this.versionNumber(), ids).toPromise();
        }
        this.snack.open('Changes saved', 'Close', { duration: 2000 });
        this.loadFields();
      } catch (e) {
        this.snack.open('Save failed — some changes may not have persisted', 'Close', { duration: 4000 });
      } finally {
        this.saving.set(false);
      }
    };
    runSequential();
  }

  // The Update DTO ships a formVersionId that the server ignores when the
  // path parameter is authoritative — we just pass empty to satisfy the
  // generated DTO's non-nullable field.
  private formIdToVersionGuid(): string {
    return '00000000-0000-0000-0000-000000000000';
  }

  iconFor(type: string): string {
    return this.toolbox.find(t => t.type === type)?.icon ?? 'help_outline';
  }

  labelFor(type: string): string {
    return this.toolbox.find(t => t.type === type)?.label ?? type;
  }

  // Bound to <input>/<textarea> in the properties panel. Uses updateSelected
  // to keep the mutation path centralised.
  bindTextInput(key: keyof DraftField, ev: Event): void {
    const val = (ev.target as HTMLInputElement | HTMLTextAreaElement)?.value ?? '';
    this.updateSelected(key, val as any);
  }
}
