import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Client, CreateFormDto, FormDto } from '../../../../core/services/api-service';
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
    MatSnackBarModule
  ],
  templateUrl: './forms-list.component.html',
  styleUrls: ['./forms-list.component.scss']
})
export class FormsListComponent implements OnInit {
  forms = signal<FormDto[]>([]);
  displayedColumns = ['name', 'description', 'createdAt', 'isActive', 'actions'];
  isLoading = signal<boolean>(false);
  error = signal<string>('');

  constructor(private apiClient: Client, private dialog: MatDialog, private snack: MatSnackBar) {}

  ngOnInit() {
    console.log('FormsListComponent initialized');
    this.loadForms();
  }

  openCreateDialog() {
    const dialogRef = this.dialog.open(CreateFormDialogComponent, {
      width: '520px',
      panelClass: 'elevated-dialog-panel',
      data: {},
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((result?: { name: string; description?: string }) => {
      if (!result) return;
      const payload: CreateFormDto = new CreateFormDto({
        name: result.name,
        description: result.description ?? '',
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
    console.log('Loading forms...');
    console.log('Token in localStorage:', localStorage.getItem('auth_token'));
    this.isLoading.set(true);
    this.error.set('');
    
    this.apiClient.formsAll().subscribe({
      next: (forms) => {
        console.log('Forms loaded:', forms);
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
      width: 'min(1100px, 95vw)',
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
    console.log('View submissions for:', form);
  }
}