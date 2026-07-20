import { Component, Inject, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
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
import { Observable } from 'rxjs';


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
	changeDetection: ChangeDetectionStrategy.Eager,
	styleUrls: ['./edit-form-dialog.component.scss']
})
export class EditFormDialogComponent implements OnInit {
	name = signal<string>('');
	description = signal<string>('');
	isActive = signal<boolean>(true);
	brandColor = signal<string>('');
	accessPassword = signal<string>('');
	showPassword = signal<boolean>(false);
	thankYouMessage = signal<string>('');
	redirectUrl = signal<string>('');
	maxSubmissions = signal<string>('');
	closesAtInput = signal<string>('');
	webhookUrl = signal<string>('');
	webhookSecret = signal<string>('');
	showWebhookSecret = signal<boolean>(false);
	oneResponsePerEmail = signal<boolean>(false);

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
			this.brandColor.set((src as any).brandColor ?? '');
			this.accessPassword.set((src as any).accessPassword ?? '');
			this.thankYouMessage.set((src as any).thankYouMessage ?? '');
			this.redirectUrl.set((src as any).redirectUrl ?? '');
			const cap = (src as any).maxSubmissions;
			this.maxSubmissions.set(cap == null ? '' : String(cap));
			this.closesAtInput.set(this.dateToLocalInput((src as any).closesAt));
			this.webhookUrl.set((src as any).webhookUrl ?? '');
			this.webhookSecret.set((src as any).webhookSecret ?? '');
			this.oneResponsePerEmail.set(!!(src as any).oneResponsePerEmail);
		}

		setWebhookUrlFromEvent(ev: Event) {
			const val = (ev.target as HTMLInputElement)?.value ?? '';
			this.webhookUrl.set(val);
		}
		setWebhookSecretFromEvent(ev: Event) {
			const val = (ev.target as HTMLInputElement)?.value ?? '';
			this.webhookSecret.set(val);
		}
		clearWebhookSecret() { this.webhookSecret.set(''); }
		webhookUrlError(): string | null {
			const v = this.webhookUrl().trim();
			if (!v) return null;
			try {
				const u = new URL(v);
				if (u.protocol !== 'http:' && u.protocol !== 'https:') return 'URL must start with http:// or https://';
				return null;
			} catch {
				return 'Enter a full URL (including https://)';
			}
		}

		// datetime-local <input> values are naive local wall-clock strings like
		// "2026-07-20T14:30". Reading a Date back needs the same shape in local
		// time; writing sends it through new Date(...) so JS interprets it as
		// local time and .toISOString() (upstream) turns it into UTC on the wire.
		private dateToLocalInput(d: Date | string | null | undefined): string {
			if (!d) return '';
			const date = d instanceof Date ? d : new Date(d);
			if (Number.isNaN(date.getTime())) return '';
			const pad = (n: number) => String(n).padStart(2, '0');
			return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
		}

		setThankYouMessageFromEvent(ev: Event) {
			const val = (ev.target as HTMLTextAreaElement)?.value ?? '';
			this.thankYouMessage.set(val);
		}
		setRedirectUrlFromEvent(ev: Event) {
			const val = (ev.target as HTMLInputElement)?.value ?? '';
			this.redirectUrl.set(val);
		}
		redirectUrlError(): string | null {
			const v = this.redirectUrl().trim();
			if (!v) return null;
			try {
				const u = new URL(v);
				if (u.protocol !== 'http:' && u.protocol !== 'https:') return 'URL must start with http:// or https://';
				return null;
			} catch {
				return 'Enter a full URL (including https://)';
			}
		}

		maxSubmissionsError(): string | null {
			const v = this.maxSubmissions().trim();
			if (!v) return null;
			const n = Number(v);
			if (!Number.isInteger(n) || n < 1) return 'Must be a whole number of 1 or more';
			return null;
		}

		setMaxSubmissionsFromEvent(ev: Event) {
			const val = (ev.target as HTMLInputElement)?.value ?? '';
			this.maxSubmissions.set(val);
		}
		setClosesAtFromEvent(ev: Event) {
			const val = (ev.target as HTMLInputElement)?.value ?? '';
			this.closesAtInput.set(val);
		}
		clearClosesAt() { this.closesAtInput.set(''); }

		clearBrandColor() { this.brandColor.set(''); }
		setAccessPasswordFromEvent(ev: Event) {
			const val = (ev.target as HTMLInputElement)?.value ?? '';
			this.accessPassword.set(val);
		}
		clearAccessPassword() { this.accessPassword.set(''); }


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
			brandColor: this.brandColor().trim() || undefined,
			accessPassword: this.accessPassword().trim() || undefined,
			thankYouMessage: this.thankYouMessage().trim() || undefined,
			redirectUrl: this.redirectUrl().trim() || undefined,
			maxSubmissions: this.maxSubmissions().trim() ? Number(this.maxSubmissions()) : undefined,
			closesAt: this.closesAtInput() ? new Date(this.closesAtInput()) : undefined,
			webhookUrl: this.webhookUrl().trim() || undefined,
			webhookSecret: this.webhookSecret().trim() || undefined,
			oneResponsePerEmail: this.oneResponsePerEmail(),
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
			const formId = this.data.form.id;
			const payload = new CreateFormVersionDto({
				description: result.description ?? '',
				fields: (result.fields ?? []).map(f => new CreateFormFieldDto({
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
				}))
			});
				this.api.versionsPOST(this.data.form.id, payload).subscribe({
				next: v => {
					const versionNumber = v.versionNumber ?? nextVersionNumber;
				const followUps: Array<{ run: () => Observable<void>; label: string }> = [];
				if (result.publish) {
					followUps.push({
						run: () => this.api.publish(formId, versionNumber),
						label: 'publish the version'
					});
				}
				if (result.makeCurrent) {
					followUps.push({
						run: () => this.api.setCurrent(formId, versionNumber),
						label: 'set the version as current'
					});
				}

				const finalize = (hadErrors: boolean) => {
					this.loadVersions();
					const requestedActions = followUps.length > 0;
					const message = hadErrors
						? 'Version created, but some follow-up actions failed'
						: requestedActions
							? 'Version created and settings applied'
							: 'Version created';
					this.snack.open(message, 'Close', { duration: hadErrors ? 3500 : 2000 });
				};

				const runFollowUp = (index: number, hadErrors: boolean) => {
					if (index >= followUps.length) {
						finalize(hadErrors);
						return;
					}
					const step = followUps[index];
					step.run().subscribe({
						next: () => {},
						error: err => {
							console.error(`Failed to ${step.label}`, err);
							runFollowUp(index + 1, true);
						},
						complete: () => runFollowUp(index + 1, hadErrors)
					});
				};

				if (followUps.length === 0) {
					finalize(false);
				} else {
					runFollowUp(0, false);
				}
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
			width: 'min(500px, 95vw)',
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

	previewVersion(v: FormVersionDto) {
		if (!this.data.form.id || v.versionNumber == null) return;
		// Open in a new tab so the admin can keep editing while the preview stays live.
		// Always pass the current dialog color state - even an empty value - so the
		// preview shows exactly what the admin is currently choosing (including a
		// reset back to the default). Param presence signals "override the DB
		// value"; the empty value case means "no brand color, use the default".
		const params = new URLSearchParams();
		params.set('brandColor', this.brandColor().trim());
		const url = `/admin/forms/${this.data.form.id}/preview/${v.versionNumber}?${params.toString()}`;
		window.open(url, '_blank');
	}

	nameError(): string | null {
		const v = (this.name() ?? '').trim();
		if (!v) return 'Name is required';
		if (v.length < 3) return 'Minimum 3 characters';
		return null;
	}

	invalid(): boolean {
		return this.nameError() != null
			|| this.redirectUrlError() != null
			|| this.maxSubmissionsError() != null
			|| this.webhookUrlError() != null;
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

