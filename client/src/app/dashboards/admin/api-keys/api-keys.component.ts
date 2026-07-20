import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DeleteDialogComponent, DeleteDialogData } from '../../../shared/delete-dialog.component/delete-dialog.component';
import { Inject } from '@angular/core';
import { environment } from '../../../../environments/environment';

interface ApiKey {
  id: string;
  name: string;
  keyPrefix: string;
  createdAt: string;
  lastUsedAt?: string | null;
  isRevoked: boolean;
}

interface ApiKeyCreated extends ApiKey {
  key: string;
}

@Component({
  selector: 'app-create-api-key-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatSnackBarModule],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>vpn_key</mat-icon>
      <span>{{ data.created ? 'API key created' : 'Create API key' }}</span>
    </h2>
    <mat-dialog-content class="dialog-content">
      @if (!data.created) {
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Name</mat-label>
          <input matInput
                 [value]="name()"
                 (input)="name.set($any($event.target).value)"
                 placeholder="e.g. Zapier integration"
                 maxlength="100" />
          <mat-hint>Give it a name so you can identify it in the list.</mat-hint>
        </mat-form-field>
      } @else {
        <p class="warn"><mat-icon>warning</mat-icon> Copy this key now &mdash; you will not be able to see it again.</p>
        <div class="key-value">{{ data.created.key }}</div>
        <p class="hint">Send it in the <code>X-Api-Key</code> header on requests to /api/forms and /api/formsubmissions read endpoints.</p>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      @if (!data.created) {
        <button mat-button (click)="close(null)">Cancel</button>
        <button mat-raised-button color="primary" (click)="submit()" [disabled]="!name().trim()">
          <mat-icon>vpn_key</mat-icon>
          <span>Create</span>
        </button>
      } @else {
        <button mat-raised-button (click)="copyKey()">
          <mat-icon>content_copy</mat-icon>
          <span>Copy key</span>
        </button>
        <button mat-raised-button color="primary" (click)="close(data.created)">Done</button>
      }
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-content { min-width: 460px; display: flex; flex-direction: column; gap: 12px; }
    .full-width { width: 100%; }
    .warn { display: flex; align-items: center; gap: 6px; color: #b45309; margin: 0; }
    .key-value {
      font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
      background: #f1f5f9;
      padding: 12px;
      border-radius: 6px;
      word-break: break-all;
      font-size: 13px;
    }
    .hint { font-size: 12px; color: #64748b; margin: 0; }
    h2 { display: flex; align-items: center; gap: 8px; }
  `],
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class CreateApiKeyDialogComponent {
  name = signal('');
  constructor(
    private ref: MatDialogRef<CreateApiKeyDialogComponent, ApiKeyCreated | null>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: { created?: ApiKeyCreated | null; onSubmit: (name: string) => void },
  ) {}
  submit(): void { this.data.onSubmit(this.name().trim()); }
  copyKey(): void {
    const raw = this.data.created?.key;
    if (!raw || !navigator.clipboard?.writeText) return;
    navigator.clipboard.writeText(raw).then(
      () => this.snack.open('Key copied', 'Close', { duration: 2000 }),
      () => this.snack.open('Copy failed', 'Close', { duration: 3000 }),
    );
  }
  close(result: ApiKeyCreated | null): void { this.ref.close(result); }
}

@Component({
  selector: 'app-api-keys',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatTableModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule,
    DatePipe,
  ],
  templateUrl: './api-keys.component.html',
  styleUrl: './api-keys.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class ApiKeysComponent {
  private http = inject(HttpClient);
  private dialog = inject(MatDialog);
  private snack = inject(MatSnackBar);

  keys = signal<ApiKey[]>([]);
  loading = signal(true);
  displayedColumns = ['name', 'keyPrefix', 'createdAt', 'lastUsedAt', 'status', 'actions'];

  constructor() { this.load(); }

  private authHeaders(): HttpHeaders {
    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('auth_token') : null;
    return token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();
  }

  private load(): void {
    this.loading.set(true);
    this.http.get<ApiKey[]>(`${environment.apiBaseUrl}/api/admin/api-keys`, { headers: this.authHeaders() })
      .subscribe({
        next: rows => { this.keys.set(rows); this.loading.set(false); },
        error: () => { this.loading.set(false); this.snack.open('Failed to load API keys', 'Close', { duration: 3000 }); },
      });
  }

  openCreate(): void {
    const ref = this.dialog.open(CreateApiKeyDialogComponent, {
      panelClass: 'elevated-dialog-panel',
      data: {
        created: null,
        onSubmit: (name: string) => {
          if (!name) return;
          this.http.post<ApiKeyCreated>(
            `${environment.apiBaseUrl}/api/admin/api-keys`,
            { name },
            { headers: this.authHeaders() },
          ).subscribe({
            next: (created) => {
              // Swap dialog into "just-created" mode showing the raw key.
              ref.componentInstance.data.created = created;
              this.load();
            },
            error: () => this.snack.open('Failed to create API key', 'Close', { duration: 3000 }),
          });
        },
      },
      disableClose: true,
    });
  }

  revoke(key: ApiKey): void {
    if (key.isRevoked) return;
    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: { itemType: 'API key', itemName: key.name } as DeleteDialogData,
      width: '420px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true,
    });
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.http.delete<ApiKey>(`${environment.apiBaseUrl}/api/admin/api-keys/${encodeURIComponent(key.id)}`, { headers: this.authHeaders() })
        .subscribe({
          next: () => { this.snack.open('API key revoked', 'Close', { duration: 2500 }); this.load(); },
          error: () => this.snack.open('Failed to revoke API key', 'Close', { duration: 3000 }),
        });
    });
  }
}
