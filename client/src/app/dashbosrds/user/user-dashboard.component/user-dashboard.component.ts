import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatSelectModule } from '@angular/material/select';
import { MatRadioModule } from '@angular/material/radio';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { SubmissionEditDialogComponent } from '../submission-edit-dialog.component/submission-edit-dialog.component';
import { Client, CreateFormSubmissionDto, FormDto, FormFieldDto, FormSubmissionDto, FormVersionDto, UpdateFormSubmissionDto } from '../../../core/services/api-service';
import { Router } from '@angular/router';
import { HeaderComponent } from '../../../shared/layout/header.component/header.component';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatListModule,
    MatButtonModule,
    MatIconModule,
    MatToolbarModule,
    MatSelectModule,
    MatCardModule,
    MatInputModule,
    MatTabsModule,
    MatTableModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatRadioModule,
    MatSnackBarModule,
    ReactiveFormsModule,
    MatDialogModule,
    HeaderComponent
  ],
  templateUrl: './user-dashboard.component.html',
  styleUrls: ['./user-dashboard.component.scss']
})
export class UserDashboardComponent implements OnInit {
  forms = signal<FormDto[]>([]);
  selectedForm = signal<FormDto | undefined>(undefined);
  versions = signal<FormVersionDto[]>([]);
  selectedVersion = signal<FormVersionDto | undefined>(undefined);
  fields = signal<FormFieldDto[]>([]);

  submissionForm = signal<FormDto | undefined>(undefined);
  submissionVersions = signal<FormVersionDto[]>([]);
  submissionVersion = signal<FormVersionDto | undefined>(undefined);
  submissionFields = signal<FormFieldDto[]>([]);
  submissions = signal<FormSubmissionDto[]>([]);
  submissionsLoading = signal(false);
  submissionsError = signal<string | null>(null);

  private submissionFieldLabelMap = new Map<string, string>();
  private submissionOptionLabelMap = new Map<string, Map<string, string>>();

  private optionsCache = new WeakMap<FormFieldDto, Array<{ label: string; value: string }>>();

  formGroup: FormGroup = new FormGroup({});
  isLoading = signal(false);
  private controlNameMap = new WeakMap<FormFieldDto, string>();
  private controlSeq = 0;

  constructor(
    private api: Client,
    private fb: FormBuilder,
    private snack: MatSnackBar,
    private router: Router,
    private dialog: MatDialog
  ){}

  logout() {
    localStorage.removeItem('auth_token');
    this.router.navigate(['/login']);
  }

  ngOnInit(): void {
    this.loadForms();
  }

  loadForms() {
    this.isLoading.set(true);
    this.api.formsAll().subscribe({
      next: (fs) => {
        this.forms.set((fs || []).filter(f => f.isActive));
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load forms', err);
        this.snack.open('Failed to load forms', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }


  selectForm(f: FormDto) {
    this.selectedForm.set(f);
    this.selectedVersion.set(undefined);
    this.fields.set([]);
    if (!f.id) return;

    this.api.versionsAll(f.id).subscribe({
      next: vs => {
        this.versions.set((vs || []).filter(v => v.isPublished));
      },
      error: err => {
        console.error('Failed to load versions', err);
        this.snack.open('Failed to load versions', 'Close', { duration: 3000 });
      }
    });
  }

  selectVersion(v: FormVersionDto) {
    this.selectedVersion.set(v);
    this.fields.set([]);
    this.formGroup = this.fb.group({});

    if (!this.selectedForm() || v.versionNumber == null) return;

    const formId = this.selectedForm()!.id!;
    this.api.fieldsAll(formId, v.versionNumber).subscribe({
      next: flds => {
        const visible = this.normalizeVisibleFields(flds);
        this.fields.set(visible);

        for (const f of visible) {
          if (f.type === 'Select' || f.type === 'Radio') {
            this.optionsCache.set(f, this.normalizedOptions(f));
          }
        }

        this.buildForm(visible);
      },
      error: err => {
        console.error('Failed to load fields', err);
        this.snack.open('Failed to load form fields', 'Close', { duration: 3000 });
      }
    });
  }

  buildForm(flds: FormFieldDto[]) {
    const group: any = {};
    for (const f of flds) {
      const name = this.fieldName(f);
      const validators = [] as any[];

      if (f.isRequired) validators.push(Validators.required);
      if (f.validation) {
        try {
          validators.push(Validators.pattern(f.validation));
        } catch {
        }
      }

      let defaultVal: any = f.defaultValue ?? '';
      if (f.type === 'Checkbox') {
        defaultVal = (f.defaultValue === 'true' || f.defaultValue === '1');
      }

      group[name] = [defaultVal, validators];
    }
    this.formGroup = this.fb.group(group);
  }

  submit() {
    if (!this.selectedVersion() || !this.selectedVersion()!.id) return;

    if (this.formGroup.invalid) {
      this.snack.open('Please fix validation errors', 'Close', { duration: 2500 });
      return;
    }

    const values: { [k: string]: string } = {};
    for (const f of this.fields()) {
      const key = this.fieldName(f);
      const val = this.formGroup.get(key)?.value;
      values[key] = (val === null || val === undefined) ? '' : String(val);
    }

    const payload = new CreateFormSubmissionDto({
      formVersionId: this.selectedVersion()!.id,
      fieldValues: values
    });

    this.api.formSubmissionsPOST(payload).subscribe({
      next: _ => {
        this.snack.open('Form submitted', 'Close', { duration: 2500 });
        this.formGroup.reset();
      },
      error: err => {
        console.error('Failed to submit', err);
        this.snack.open('Failed to submit form', 'Close', { duration: 3000 });
      }
    });
  }

  normalizedOptions(f: FormFieldDto): Array<{ label: string; value: string }> {
    const result: Array<{ label: string; value: string }> = [];
    let raw: any = (f as any).options;

    if (raw == null) return result;

    if (typeof raw === 'string') {
      const trimmed = raw.trim();
      if (!trimmed) return result;
      try {
        const parsed = JSON.parse(trimmed);
        raw = parsed;
      } catch {
        return trimmed
          .split(',')
          .map(s => s.trim())
          .filter(s => s.length)
          .map(s => ({ label: s, value: s }));
      }
    }

    if (Array.isArray(raw)) {
      for (const item of raw) {
        if (item && typeof item === 'object') {
          const label = String(
            (item as any).label ??
            (item as any).name ??
            (item as any).text ??
            (item as any).value ??
            '[option]'
          );
          const value = String((item as any).value ?? (item as any).id ?? label);
          result.push({ label, value });
        } else {
          const s = String(item);
          result.push({ label: s, value: s });
        }
      }

      const sequentialNumericLabels = result.length > 0 && result.every((opt, idx) => {
        const trimmed = opt.label?.trim?.() ?? '';
        return /^\d+$/.test(trimmed) && Number(trimmed) === idx + 1;
      });
      if (sequentialNumericLabels) {
        return result.map(opt => ({ label: opt.value, value: opt.value }));
      }

      return result;
    }

    if (typeof raw === 'object') {
      for (const [k, v] of Object.entries(raw)) {
        result.push({ label: String(v), value: String(k) });
      }
    }

    return result;
  }

  private normalizeVisibleFields(fields?: FormFieldDto[]): FormFieldDto[] {
    return (fields || [])
      .filter(f => f.isVisible !== false)
      .sort((a, b) => (a.order || 0) - (b.order || 0));
  }

  getOptions(f: FormFieldDto): Array<{ label: string; value: string }> {
    return this.optionsCache.get(f) ?? [];
  }

  
  fieldName(f: FormFieldDto): string {
    const cached = this.controlNameMap.get(f);
    if (cached) return cached;

    const fromName = f.name;
    if (fromName && String(fromName).length) {
      const v = String(fromName);
      this.controlNameMap.set(f, v);
      return v;
    }

    const fromId = f.id;
    if (fromId !== undefined && fromId !== null) {
      const v = String(fromId);
      this.controlNameMap.set(f, v);
      return v;
    }

    const fromOrder = f.order;
    if (fromOrder !== undefined && fromOrder !== null) {
      const v = `field_${String(fromOrder)}`;
      this.controlNameMap.set(f, v);
      return v;
    }

    const v = `field_auto_${++this.controlSeq}`;
    this.controlNameMap.set(f, v);
    return v;
  }


  selectSubmissionForm(form: FormDto) {
    this.submissionForm.set(form);
    this.submissionVersion.set(undefined);
    this.submissions.set([]);
    this.submissionFields.set([]);
    this.submissionsError.set(null);
    this.submissionFieldLabelMap.clear();
    this.submissionOptionLabelMap.clear();

    if (!form.id) {
      this.submissionVersions.set([]);
      return;
    }

    this.api.versionsAll(form.id).subscribe({
      next: vs => {
        this.submissionVersions.set((vs || []).filter(v => v.isPublished));
      },
      error: err => {
        console.error('Failed to load versions', err);
        this.snack.open('Failed to load versions', 'Close', { duration: 3000 });
        this.submissionVersions.set([]);
      }
    });
  }

  selectSubmissionVersion(version: FormVersionDto) {
    this.submissionVersion.set(version);
    this.submissions.set([]);
    this.submissionFields.set([]);
    this.submissionsError.set(null);
    this.submissionFieldLabelMap.clear();
    this.submissionOptionLabelMap.clear();

    const form = this.submissionForm();
    if (!form || version.versionNumber == null) {
      return;
    }

    const versionId = version.id;
    if (!versionId) {
      this.snack.open('Selected version is missing an identifier', 'Close', { duration: 3000 });
      return;
    }

    this.submissionsLoading.set(true);
    this.fetchSubmissions(versionId);

    this.api.fieldsAll(form.id!, version.versionNumber).subscribe({
      next: fields => {
        const visible = this.normalizeVisibleFields(fields);
        this.submissionFields.set(visible);
        this.rebuildSubmissionFieldMaps(visible);
        console.log('[UserDashboard] submissionFields loaded', visible.length);
      },
      error: err => {
        console.error('Failed to load fields', err);
        this.snack.open('Failed to load form fields', 'Close', { duration: 3000 });
      }
    });
  }

  private fetchSubmissions(versionId: string) {
    this.api.formVersion(versionId).subscribe({
      next: subs => {
        this.submissions.set(subs || []);
        this.submissionsLoading.set(false);
      },
      error: err => {
        console.error('Failed to load submissions', err);
        this.submissionsError.set('Failed to load submissions');
        this.snack.open('Failed to load submissions', 'Close', { duration: 3000 });
        this.submissionsLoading.set(false);
      }
    });
  }

  private rebuildSubmissionFieldMaps(fields: FormFieldDto[]) {
    this.submissionFieldLabelMap.clear();
    this.submissionOptionLabelMap.clear();

    for (const f of fields) {
      const key = this.submissionFieldKey(f);
      if (!key) continue;

      const label = f.label?.trim().length ? f.label : (f.name ?? key);
      this.submissionFieldLabelMap.set(key, label ?? key);

      if (f.type === 'Select' || f.type === 'Radio') {
        const options = this.normalizedOptions(f);
        const optionMap = new Map<string, string>();
        for (const opt of options) {
          optionMap.set(opt.value, opt.label);
        }
        this.submissionOptionLabelMap.set(key, optionMap);
      }
    }
  }

  private submissionFieldKey(f: FormFieldDto): string | null {
    if (f.name && String(f.name).length) {
      return String(f.name);
    }
    if (f.id !== undefined && f.id !== null) {
      return String(f.id);
    }
    if (f.order !== undefined && f.order !== null) {
      return `field_${String(f.order)}`;
    }
    return null;
  }

  submissionFieldLabel(name?: string): string {
    if (!name) return '';
    return this.submissionFieldLabelMap.get(name) ?? name;
  }

  submissionValueLabel(fieldName: string | undefined, value: string | undefined): string {
    if (!fieldName) return value ?? '';
    if (value == null) return '';
    const map = this.submissionOptionLabelMap.get(fieldName);
    if (map && map.has(value)) {
      return map.get(value)!;
    }
    return value;
  }


  openSubmissionEditDialog(submission: FormSubmissionDto) {
    console.log('[UserDashboard] openSubmissionEditDialog click', submission);

    if (!submission.id) {
      this.snack.open('Selected submission is missing an identifier', 'Close', { duration: 3000 });
      return;
    }

    const form = this.submissionForm();
    const version = this.submissionVersion();

    if (!form || !version) {
      this.snack.open('Select a form and version before editing submissions', 'Close', { duration: 3000 });
      return;
    }

    const fields = this.submissionFields();

    if (!fields.length) {
      console.warn('[UserDashboard] submissionFields are empty when clicking edit');
      this.snack.open('Please wait until fields are loaded for this version', 'Close', { duration: 3000 });
      return;
    }

    this.rebuildSubmissionFieldMaps(fields);

    console.log('[UserDashboard] opening submission edit dialog with fields', fields.length);
    this.launchSubmissionEditDialog(submission, fields);
  }

  private launchSubmissionEditDialog(submission: FormSubmissionDto, fields: FormFieldDto[]): void {
    console.log('[UserDashboard] launchSubmissionEditDialog: opening dialog');

    const dialogRef = this.dialog.open(SubmissionEditDialogComponent, {
      width: 'min(1100px, 95vw)',
      maxWidth: '95vw',
      height: '80vh',
      panelClass: 'wide-dialog-panel', 
      data: { submission, fields },
      disableClose: true
    });

    dialogRef.afterOpened().subscribe(() => {
      const container = document.querySelector('mat-dialog-container');
      console.log('[UserDashboard] submission dialog opened, container present?', !!container);
      if (container) {
        console.log(
          '[UserDashboard] submission dialog container rect',
          (container as HTMLElement).getBoundingClientRect()
        );
      }
    });

    dialogRef.afterClosed().subscribe((result?: { fieldValues: { [key: string]: string } }) => {
      if (!result) {
        console.log('[UserDashboard] submission dialog closed without result');
        return;
      }

      const payload = new UpdateFormSubmissionDto({
        submitterName: submission.submitterName,
        submitterEmail: submission.submitterEmail,
        fieldValues: result.fieldValues
      });

      const versionId = this.submissionVersion()?.id;

      this.api.formSubmissionsPUT(submission.id!, payload).subscribe({
        next: () => {
          this.snack.open('Submission updated', 'Close', { duration: 2500 });
          if (versionId) {
            this.submissionsLoading.set(true);
            this.fetchSubmissions(versionId);
          }
        },
        error: err => {
          console.error('Failed to update submission', err);
          this.snack.open('Failed to update submission', 'Close', { duration: 3000 });
        }
      });
    });
  }


  deleteSubmission(submission: FormSubmissionDto) {
    if (!submission.id) {
      this.snack.open('Selected submission is missing an identifier', 'Close', { duration: 3000 });
      return;
    }

    const confirmed = window.confirm('Delete this submission?');
    if (!confirmed) {
      return;
    }

    const versionId = this.submissionVersion()?.id;
    this.api.formSubmissionsDELETE(submission.id).subscribe({
      next: () => {
        this.snack.open('Submission deleted', 'Close', { duration: 2500 });
        if (versionId) {
          this.submissionsLoading.set(true);
          this.fetchSubmissions(versionId);
        }
      },
      error: err => {
        console.error('Failed to delete submission', err);
        this.snack.open('Failed to delete submission', 'Close', { duration: 3000 });
      }
    });
  }
}
