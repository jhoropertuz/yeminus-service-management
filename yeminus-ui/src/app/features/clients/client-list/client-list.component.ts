import { Component, inject, OnInit } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { NgIf } from '@angular/common';
import { Client } from '../../../shared/models/client.model';
import { ClientService } from '../../../core/services/client.service';
import { ClientFormComponent } from '../client-form/client-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [
    MatTableModule, MatButtonModule, MatIconModule, MatSnackBarModule,
    MatProgressSpinnerModule, MatCardModule, NgIf
  ],
  template: `
    <div class="page-container">
      <div class="action-bar">
        <h2>Clients</h2>
        <button mat-flat-button color="primary" (click)="openForm()">
          <mat-icon>add</mat-icon> New Client
        </button>
      </div>

      <mat-card *ngIf="loading">
        <mat-card-content style="text-align:center;padding:32px">
          <mat-spinner diameter="40" style="margin:auto"></mat-spinner>
        </mat-card-content>
      </mat-card>

      <mat-card *ngIf="!loading">
        <mat-card-content>
          <table mat-table [dataSource]="clients" class="full-width">
            <ng-container matColumnDef="fullName">
              <th mat-header-cell *matHeaderCellDef>Name</th>
              <td mat-cell *matCellDef="let c">{{ c.fullName }}</td>
            </ng-container>
            <ng-container matColumnDef="documentNumber">
              <th mat-header-cell *matHeaderCellDef>Document</th>
              <td mat-cell *matCellDef="let c">{{ c.documentNumber }}</td>
            </ng-container>
            <ng-container matColumnDef="phone">
              <th mat-header-cell *matHeaderCellDef>Phone</th>
              <td mat-cell *matCellDef="let c">{{ c.phone }}</td>
            </ng-container>
            <ng-container matColumnDef="email">
              <th mat-header-cell *matHeaderCellDef>Email</th>
              <td mat-cell *matCellDef="let c">{{ c.email }}</td>
            </ng-container>
            <ng-container matColumnDef="address">
              <th mat-header-cell *matHeaderCellDef>Address</th>
              <td mat-cell *matCellDef="let c">{{ c.address }}</td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let c">
                <button mat-icon-button color="primary" (click)="openForm(c)" title="Edit">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button color="warn" (click)="deleteClient(c)" title="Delete">
                  <mat-icon>delete</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    </div>
  `
})
export class ClientListComponent implements OnInit {
  private clientService = inject(ClientService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  clients: Client[] = [];
  loading = false;
  displayedColumns = ['fullName', 'documentNumber', 'phone', 'email', 'address', 'actions'];

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.clientService.getAll().subscribe({
      next: clients => { this.clients = clients; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  openForm(client?: Client): void {
    const ref = this.dialog.open(ClientFormComponent, {
      width: '520px', data: { client }
    });
    ref.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open(client ? 'Client updated.' : 'Client created.', 'Close', { duration: 3000 });
        this.load();
      }
    });
  }

  deleteClient(client: Client): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete Client', message: `Are you sure you want to delete ${client.fullName}?` }
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.clientService.delete(client.id).subscribe({
          next: () => {
            this.snackBar.open('Client deleted.', 'Close', { duration: 3000 });
            this.load();
          },
          error: err => this.snackBar.open(err.error?.message ?? 'Error deleting client.', 'Close', { duration: 4000 })
        });
      }
    });
  }
}
