import { Component, Inject, OnInit, signal } from '@angular/core';
import { Client, FormDto, UpdateFormDto, FormVersionDto, CreateFormVersionDto, UpdateFormVersionDto, CreateFormFieldDto } from '../../../../core/services/api-service';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CreateVersionDialogComponent } from '../create-version-dialog.component/create-version-dialog.component';
import { ManageFieldsDialogComponent } from '../manage-fields-dialog.component/manage-fields-dialog.component';
import { DeleteDialogComponent, DeleteDialogData } from '../../../../shared/delete-dialog.component/delete-dialog.component';
import { EditVersionDialogComponent } from '../edit-version-dialog.component/edit-version-dialog.component';


export type EditFormDialogData = {
	form: FormDto;
};

@Component({
	selector: 'app-edit-form-dialog',
	standalone: true,
	imports: [
		CommonModule,
		MatDialogModule,
		MatFormFieldModule,
		MatInputModule,
		MatIconModule,
		MatButtonModule,
		MatTableModule,
		MatChipsModule,
		MatCardModule,
		MatTooltipModule,
		MatSlideToggleModule,
		MatSnackBarModule
	],
	templateUrl: './edit-form-dialog.component.html',
	styleUrls: ['./edit-form-dialog.component.scss']
})
export class EditFormDialogComponent implements OnInit {
	name = signal<string>('');
	description = signal<string>('');
	isActive = signal<boolean>(true);
	
	nameTouched = signal<boolean>(false);
	isSaving = signal(false);
	versions = signal<FormVersionDto[]>([]);
	displayedColumns = ['versionNumber','description','createdAt','isPublished','isCurrent','actions'];

		constructor(
			private api: Client,
			private snack: MatSnackBar,
			private dialog: MatDialog,
			private dialogRef: MatDialogRef<EditFormDialogComponent>,
			@Inject(MAT_DIALOG_DATA) public data: EditFormDialogData,
		) {
			const src = data.form;
			this.name.set(src.name ?? '');
			this.description.set(src.description ?? '');
			this.isActive.set(!!src.isActive);
		}


	ngOnInit(): void {
		this.loadVersions();
	}

	loadVersions() {
		if (!this.data.form.id) return;
		this.api.versionsAll(this.data.form.id).subscribe({
			next: v => this.versions.set(v),
			error: err => {
				console.error('Failed to load versions', err);
				this.snack.open('Failed to load versions', 'Close', { duration: 2500 });
			}
		});
	}

	save() {
		if (!this.data.form.id || this.invalid() || this.isSaving()) return;
		this.isSaving.set(true);
		const payload = new UpdateFormDto({
			name: this.name(),
			description: this.description(),
			isActive: this.isActive(),
		});
		this.api.formsPUT(this.data.form.id, payload).subscribe({
			next: updated => {
				this.snack.open('Form updated', 'Close', { duration: 2000 });
				this.dialogRef.close({ updated });
			},
			error: err => {
				console.error('Failed to update form', err);
				this.snack.open('Failed to update form', 'Close', { duration: 3000 });
				this.isSaving.set(false);
			}
		});
	}

	close() {
		this.dialogRef.close();
	}

	addVersion() {
		const nextVersionNumber = (this.versions()?.length || 0) + 1;
	    const ref = this.dialog.open(CreateVersionDialogComponent, {
			    width: '550px',
				maxWidth: '85vw',
				height: '50vh',
				panelClass: 'elevated-dialog-panel',
				disableClose: true,
					data: { nextVersionNumber }
			});
			ref.afterClosed().subscribe((result?: { description?: string, fields?: Array<any>, publish?: boolean, makeCurrent?: boolean }) => {
			if (!result || !this.data.form.id) return;
				const payload = new CreateFormVersionDto({
					description: result.description ?? '',
					fields: [] 
				});
				this.api.versionsPOST(this.data.form.id, payload).subscribe({
				next: v => {
					const versionNumber = v.versionNumber ?? nextVersionNumber;
					this.snack.open('Version created', 'Close', { duration: 2000 });
					this.loadVersions();
					this.dialog.open(ManageFieldsDialogComponent, {
						width: '600px',
						maxWidth: '85vw',
						height: '70vh',
						panelClass: 'elevated-dialog-panel',
						data: { formId: this.data.form!.id!, versionNumber },
						disableClose: false
					});
				},
				error: err => {
					console.error('Failed to create version', err);
					this.snack.open('Failed to create version', 'Close', { duration: 3000 });
				}
			});
		});
	}

	editVersion(v: FormVersionDto) {
		const ref = this.dialog.open(EditVersionDialogComponent, {
			width: 'min(1000px, 95vw)',
			maxWidth: '95vw',
			height: 'auto',
			maxHeight: '60vh',
			panelClass: 'wide-dialog-panel',
			disableClose: true,
			data: { description: v.description ?? '', formId: this.data.form.id!, versionNumber: v.versionNumber! }
		});
		ref.afterClosed().subscribe((result?: { description?: string, fields?: Array<{name:string,label:string,type:string}> }) => {
			if (!result || !this.data.form.id || v.versionNumber == null) return;
			const updateDesc = new UpdateFormVersionDto({ description: result.description ?? '' });
			const addFields = result.fields || [];
			let hadError = false;
			const ops: Array<Promise<void>> = [];
			ops.push(new Promise((resolve) => this.api.versionsPUT(this.data.form!.id!, v.versionNumber!, updateDesc).subscribe({ next: () => resolve(), error: () => { hadError = true; resolve(); } })));
			if (addFields.length) {
				ops.push(new Promise((resolve) => {
					Promise.all(
						addFields.map((f:any) => new Promise<void>((res) => this.api.fieldsPOST(this.data.form!.id!, v.versionNumber!, new CreateFormFieldDto({
							name: f.name,
							label: f.label,
							type: f.type,
							order: f.order ?? 0,
							isRequired: !!f.isRequired,
							isVisible: f.isVisible !== false,
							isReadOnly: !!f.isReadOnly,
							placeholder: f.placeholder || '',
							helpText: f.helpText || '',
							defaultValue: f.defaultValue || '',
							validation: f.validation || '',
							options: f.options || ''
						})).subscribe({ next: () => res(), error: () => { hadError = true; res(); } })))
					).then(() => resolve());
				}));
			}
			Promise.all(ops).then(() => {
				if (hadError) {
					this.snack.open('Some changes failed to save', 'Close', { duration: 3000 });
				} else {
					this.snack.open('Version updated', 'Close', { duration: 2000 });
				}
				this.loadVersions();
			});
		});
	}

	publishVersion(v: FormVersionDto) {
		if (!this.data.form.id || v.versionNumber == null) return;
		this.api.publish(this.data.form.id, v.versionNumber).subscribe({
			next: _ => {
				this.snack.open('Version published', 'Close', { duration: 2000 });
				this.loadVersions();
			},
			error: err => {
				console.error('Failed to publish version', err);
				this.snack.open('Failed to publish version', 'Close', { duration: 3000 });
			}
		});
	}

	makeCurrent(v: FormVersionDto) {
		if (!this.data.form.id || v.versionNumber == null) return;
		this.api.setCurrent(this.data.form.id, v.versionNumber).subscribe({
			next: _ => {
				this.snack.open('Set as current', 'Close', { duration: 2000 });
				this.loadVersions();
			},
			error: err => {
				console.error('Failed to set current version', err);
				this.snack.open('Failed to set current version', 'Close', { duration: 3000 });
			}
		});
	}

	deleteVersion(v: FormVersionDto) {
			if (!this.data.form.id || v.versionNumber == null) return;
			const dialogRef = this.dialog.open(DeleteDialogComponent, {
				data: {
					itemType: 'version',
					itemName: `v${v.versionNumber}`
				} as DeleteDialogData,
				width: '400px',
				panelClass: 'elevated-dialog-panel',
				disableClose: true
			});

			dialogRef.afterClosed().subscribe((confirmed: boolean) => {
				if (!confirmed) return;
				this.api.versionsDELETE(this.data.form.id!, v.versionNumber!).subscribe({
					next: _ => {
						this.snack.open('Version deleted', 'Close', { duration: 2000 });
						this.loadVersions();
					},
					error: err => {
						console.error('Failed to delete version', err);
						this.snack.open('Failed to delete version', 'Close', { duration: 3000 });
					}
				});
			});
	}

	manageFields(v: FormVersionDto) {
		if (!this.data.form.id || v.versionNumber == null) return;
		this.dialog.open(ManageFieldsDialogComponent, {
			width: '600px',
			maxWidth: '85vw',
			height: '70vh',
			panelClass: 'elevated-dialog-panel',
			data: { formId: this.data.form.id!, versionNumber: v.versionNumber! },
			disableClose: false
		});
	}

	nameError(): string | null {
		const v = (this.name() ?? '').trim();
		if (!v) return 'Name is required';
		if (v.length < 3) return 'Minimum 3 characters';
		return null;
	}

	invalid(): boolean {
		return this.nameError() != null;
	}

	setNameFromEvent(ev: Event) {
		const val = (ev.target as HTMLInputElement)?.value ?? '';
		this.name.set(val);
	}
	setDescriptionFromEvent(ev: Event) {
		const val = (ev.target as HTMLTextAreaElement)?.value ?? '';
		this.description.set(val);
	}
}

