import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';

// GET/PUT /api/workspace shape. Not in the generated API client yet.
interface Workspace {
  id: string;
  name: string;
  slug: string;
  createdAt: string;
  userCount: number;
  formCount: number;
}

@Component({
  selector: 'app-workspace-settings',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
  ],
  templateUrl: './workspace-settings.component.html',
  styleUrl: './workspace-settings.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class WorkspaceSettingsComponent implements OnInit {
  workspace = signal<Workspace | null>(null);
  isLoading = signal(false);
  isSaving = signal(false);
  editableName = signal('');

  private http = inject(HttpClient);
  private snack = inject(MatSnackBar);
  private auth = inject(AuthService);

  private get authHeaders(): HttpHeaders {
    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('auth_token') : null;
    return token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.http.get<Workspace>(`${environment.apiBaseUrl}/api/workspace`, { headers: this.authHeaders })
      .subscribe({
        next: (w) => {
          this.workspace.set(w);
          this.editableName.set(w?.name ?? '');
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.snack.open('Failed to load workspace settings', 'Close', { duration: 3000 });
        },
      });
  }

  save(): void {
    const name = this.editableName().trim();
    if (!name) {
      this.snack.open('Workspace name is required', 'Close', { duration: 3000 });
      return;
    }
    if (name === this.workspace()?.name) {
      // Nothing to save — same value.
      return;
    }
    this.isSaving.set(true);
    this.http.put<Workspace>(`${environment.apiBaseUrl}/api/workspace`, { name }, { headers: this.authHeaders })
      .subscribe({
        next: (w) => {
          this.workspace.set(w);
          this.editableName.set(w?.name ?? '');
          // Push the new workspace name onto the auth signal so the
          // header chip updates immediately without waiting for a
          // fresh login. Also persists to localStorage via setUser.
          const current = this.auth.user();
          if (current && w?.name) {
            this.auth.setUser({ ...current, organizationName: w.name });
          }
          this.isSaving.set(false);
          this.snack.open('Workspace renamed', 'Close', { duration: 2500 });
        },
        error: () => {
          this.isSaving.set(false);
          this.snack.open('Failed to rename workspace', 'Close', { duration: 3000 });
        },
      });
  }

  copyToClipboard(text: string, label: string): void {
    if (!text) return;
    navigator.clipboard.writeText(text).then(
      () => this.snack.open(`${label} copied`, 'Close', { duration: 1500 }),
      () => this.snack.open('Failed to copy', 'Close', { duration: 2000 })
    );
  }
}
