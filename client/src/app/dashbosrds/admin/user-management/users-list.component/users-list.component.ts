import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { Client, UserDto, UserRole } from '../../../../core/services/api-service';
import { UserDialogComponent } from '../user-dialog.component/user-dialog.component';
import { DeleteDialogComponent, DeleteDialogData } from '../../../../shared/delete-dialog.component/delete-dialog.component';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatDialogModule
  ],
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.scss']
})
export class UsersListComponent implements OnInit {
  users = signal<UserDto[]>([]);
  displayedColumns = ['username', 'email', 'roles', 'createdAt', 'actions'];
  isLoading = signal<boolean>(false);
  error = signal<string>('');
  
  UserRole = UserRole;

  constructor(private apiClient: Client, private dialog: MatDialog) {}

  ngOnInit() {
    console.log('UsersListComponent initialized');
    this.loadUsers();
  }

  loadUsers() {
    console.log('Loading users...');
    this.isLoading.set(true);
    this.error.set('');
    
    this.apiClient.userAll().subscribe({
      next: (users) => {
        console.log('Users loaded:', users);
        this.users.set(users);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading users:', err);
        this.error.set('Failed to load users');
        this.isLoading.set(false);
      }
    });
  }

  createUser() {
    const ref = this.dialog.open(UserDialogComponent, {
      data: { mode: 'create' },
      width: '600px',
      maxWidth: '85vw',
      panelClass: 'elevated-dialog-panel'
    });
    ref.afterClosed().subscribe((created: UserDto | null) => {
      if (created) {
        this.users.set([created, ...this.users()]);
      }
    });
  }

  editUser(user: UserDto) {
    const ref = this.dialog.open(UserDialogComponent, {
      data: { mode: 'edit', user },
      width: '600px',
      maxWidth: '85vw',
      panelClass: 'elevated-dialog-panel'
    });
    ref.afterClosed().subscribe((updated: UserDto | null) => {
      if (updated) {
        const arr = this.users().map(u => u.id === updated.id ? updated : u);
        this.users.set(arr);
      }
    });
  }

  deleteUser(user: UserDto) {
    if (!user?.id) return;
    const userId = user.id as string; 
    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        itemType: 'user',
        itemName: user.username
      } as DeleteDialogData,
      width: '400px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.apiClient.userDELETE(userId).subscribe({
        next: () => {
          this.users.set(this.users().filter(u => u.id !== userId));
          
        },
        error: (err) => {
          console.error('Error deleting user:', err);         
        }
      });
    });
  }
}