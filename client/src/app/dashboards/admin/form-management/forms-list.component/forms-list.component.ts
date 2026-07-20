import { CommonModule } from '@angular/common';
import { Component, OnInit, signal, ChangeDetectionStrategy, inject, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Client, CreateFormDto, FormDto } from '../../../../core/services/api-service';
import { environment } from '../../../../../environments/environment';
import { CreateFormDialogComponent } from '../create-form-dialog.component/create-form-dialog.component';
import { EditFormDialogComponent } from '../edit-form-dialog.component/edit-form-dialog.component';
import { DeleteDialogComponent, DeleteDialogData } from '../../../../shared/delete-dialog.component/delete-dialog.component';

@Component({
  selector: 'app-forms-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './forms-list.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./forms-list.component.scss']
})
export class FormsListComponent implements OnInit {
  forms = signal<FormDto[]>([]);
  displayedColumns = ['name', 'description', 'createdAt', 'isActive', 'actions'];
  isLoading = signal<boolean>(false);
  error = signal<string>('');
  copiedSlug = signal<string | null>(null);

  searchQuery = signal<string>('');
  activeFilter = signal<'all' | 'active' | 'inactive'>('all');

  filteredForms = computed<FormDto[]>(() => {
    const q = this.searchQuery().trim().toLowerCase();
    const status = this.activeFilter();
    return this.forms().filter(f => {
      if (status === 'active' && !f.isActive) return false;
      if (status === 'inactive' && f.isActive) return false;
      if (!q) return true;
      const name = (f.name ?? '').toLowerCase();
      const desc = (f.description ?? '').toLowerCase();
      return name.includes(q) || desc.includes(q);
    });
  });

  setSearch(value: string): void { this.searchQuery.set(value); }
  clearSearch(): void { this.searchQuery.set(''); }
  setFilter(value: 'all' | 'active' | 'inactive'): void { this.activeFilter.set(value); }

  private router = inject(Router);
  private http = inject(HttpClient);

  constructor(private apiClient: Client, private dialog: MatDialog, private snack: MatSnackBar) {}

  duplicateForm(form: FormDto) {
    if (!form.id) return;
    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('auth_token') : null;
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();
    this.isLoading.set(true);
    this.http.post<FormDto>(`${environment.apiBaseUrl}/api/forms/${form.id}/duplicate`, {}, { headers }).subscribe({
      next: () => {
        this.snack.open(`Duplicated "${form.name}"`, 'Close', { duration: 2500 });
        this.loadForms();
      },
      error: () => {
        this.snack.open('Failed to duplicate form', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }

  canShare(form: FormDto): boolean {
    if (!form.slug || !form.isActive) return false;
    return (form.versions ?? []).some(v => v.isCurrentVersion && v.isPublished);
  }

  copyShareLink(form: FormDto) {
    if (!this.canShare(form) || !form.slug) return;
    const url = `${window.location.origin}/f/${form.slug}`;
    const done = () => {
      this.copiedSlug.set(form.slug!);
      this.snack.open('Public link copied', 'Close', { duration: 2500 });
      setTimeout(() => this.copiedSlug.set(null), 2500);
    };
    if (navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(url).then(done, () => this.snack.open('Copy failed: ' + url, 'Close', { duration: 5000 }));
    } else {
      done();
      prompt('Copy this link:', url);
    }
  }

  ngOnInit() {
    this.loadForms();
  }

  openCreateDialog() {
    const dialogRef = this.dialog.open(CreateFormDialogComponent, {
      width: '520px',
      panelClass: 'elevated-dialog-panel',
      data: {},
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((result?: { name: string; description?: string; brandColor?: string; accessPassword?: string; thankYouMessage?: string; redirectUrl?: string; maxSubmissions?: number; closesAt?: Date }) => {
      if (!result) return;
      const payload: CreateFormDto = new CreateFormDto({
        name: result.name,
        description: result.description ?? '',
        brandColor: result.brandColor || undefined,
        accessPassword: result.accessPassword || undefined,
        thankYouMessage: result.thankYouMessage || undefined,
        redirectUrl: result.redirectUrl || undefined,
        maxSubmissions: result.maxSubmissions,
        closesAt: result.closesAt,
      });
      this.isLoading.set(true);
      this.apiClient.formsPOST(payload).subscribe({
        next: (_) => {
          this.snack.open('Form created successfully', 'Close', { duration: 2500 });
          this.loadForms();
        },
        error: (err) => {
          console.error('Failed to create form', err);
          this.snack.open('Failed to create form', 'Close', { duration: 3000 });
          this.isLoading.set(false);
        }
      });
    });
  }

  loadForms() {
    this.isLoading.set(true);
    this.error.set('');
    
    this.apiClient.formsAll().subscribe({
      next: (forms) => {
        this.forms.set(forms);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading forms:', err);
        console.error('Error details:', err.message, err.status, err.response);
        this.error.set('Failed to load forms: ' + (err.message || 'Unknown error'));
        this.isLoading.set(false);
      }
    });
  }

  editForm(form: FormDto) {
    const dialogRef = this.dialog.open(EditFormDialogComponent, {
      width: 'min(1000px, 95vw)',
      maxWidth: '95vw',
      height: '80vh',
      panelClass: 'wide-dialog-panel',
      data: { form },
      disableClose: true
    });
    dialogRef.afterClosed().subscribe((result?: { updated?: FormDto }) => {
      if (result?.updated) {
        this.loadForms();
      }
    });
  }

  deleteForm(form: FormDto) {
    if (!form.id) return;
    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        itemType: 'form',
        itemName: form.name
      } as DeleteDialogData,
      width: '400px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.isLoading.set(true);
      this.apiClient.formsDELETE(form.id!).subscribe({
        next: _ => {
          this.snack.open('Form deleted successfully', 'Close', { duration: 2500 });
          this.loadForms();
        },
        error: err => {
          console.error('Failed to delete form', err);
          this.snack.open('Failed to delete form', 'Close', { duration: 3000 });
          this.isLoading.set(false);
        }
      });
    });
  }

  viewSubmissions(form: FormDto) {
    if (!form.id) return;
    this.router.navigate(['/admin/forms', form.id, 'submissions']);
  }
}