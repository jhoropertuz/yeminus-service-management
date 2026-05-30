import { Component, inject, OnInit } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { NgIf } from '@angular/common';
import { Technician } from '../../../shared/models/technician.model';
import { TechnicianService } from '../../../core/services/technician.service';
import { TechnicianFormComponent } from '../technician-form/technician-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-technician-list',
  standalone: true,
  imports: [
    MatTableModule, MatButtonModule, MatIconModule, MatSnackBarModule,
    MatProgressSpinnerModule, MatCardModule, NgIf
  ],
  template: `
    <div class="page-container">
      <div class="action-bar">
        <h2>Technicians</h2>
        <button mat-flat-button color="primary" (click)="openForm()">
          <mat-icon>add</mat-icon> New Technician
        </button>
      </div>

      <mat-card *ngIf="loading">
        <mat-card-content style="text-align:center;padding:32px">
          <mat-spinner diameter="40" style="margin:auto"></mat-spinner>
        </mat-card-content>
      </mat-card>

      <mat-card *ngIf="!loading">
        <mat-card-content>
          <table mat-table [dataSource]="technicians" class="full-width">
            <ng-container matColumnDef="fullName">
              <th mat-header-cell *matHeaderCellDef>Name</th>
              <td mat-cell *matCellDef="let t">{{ t.fullName }}</td>
            </ng-container>
            <ng-container matColumnDef="documentNumber">
              <th mat-header-cell *matHeaderCellDef>Document</th>
              <td mat-cell *matCellDef="let t">{{ t.documentNumber }}</td>
            </ng-container>
            <ng-container matColumnDef="specialty">
              <th mat-header-cell *matHeaderCellDef>Specialty</th>
              <td mat-cell *matCellDef="let t">{{ t.specialty }}</td>
            </ng-container>
            <ng-container matColumnDef="phone">
              <th mat-header-cell *matHeaderCellDef>Phone</th>
              <td mat-cell *matCellDef="let t">{{ t.phone }}</td>
            </ng-container>
            <ng-container matColumnDef="email">
              <th mat-header-cell *matHeaderCellDef>Email</th>
              <td mat-cell *matCellDef="let t">{{ t.email }}</td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let t">
                <button mat-icon-button color="primary" (click)="openForm(t)"><mat-icon>edit</mat-icon></button>
                <button mat-icon-button color="warn" (click)="deleteTechnician(t)"><mat-icon>delete</mat-icon></button>
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
export class TechnicianListComponent implements OnInit {
  private technicianService = inject(TechnicianService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  technicians: Technician[] = [];
  loading = false;
  displayedColumns = ['fullName', 'documentNumber', 'specialty', 'phone', 'email', 'actions'];

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.technicianService.getAll().subscribe({
      next: t => { this.technicians = t; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  openForm(technician?: Technician): void {
    const ref = this.dialog.open(TechnicianFormComponent, { width: '520px', data: { technician } });
    ref.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open(technician ? 'Technician updated.' : 'Technician created.', 'Close', { duration: 3000 });
        this.load();
      }
    });
  }

  deleteTechnician(technician: Technician): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete Technician', message: `Delete ${technician.fullName}?` }
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.technicianService.delete(technician.id).subscribe({
          next: () => { this.snackBar.open('Technician deleted.', 'Close', { duration: 3000 }); this.load(); },
          error: err => this.snackBar.open(err.error?.message ?? 'Error.', 'Close', { duration: 4000 })
        });
      }
    });
  }
}
