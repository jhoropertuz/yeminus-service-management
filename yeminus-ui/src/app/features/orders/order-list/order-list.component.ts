import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NgIf, NgClass, DatePipe } from '@angular/common';
import { Order, OrderFilter, OrderStatus } from '../../../shared/models/order.model';
import { OrderService } from '../../../core/services/order.service';
import { OrderFormComponent } from '../order-form/order-form.component';
import { OrderStatusComponent } from '../order-status/order-status.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatSnackBarModule,
    MatProgressSpinnerModule, MatCardModule, MatChipsModule, MatTooltipModule,
    NgIf, NgClass, DatePipe
  ],
  template: `
    <div class="page-container">
      <div class="action-bar">
        <h2>Service Orders</h2>
        <button mat-flat-button color="primary" (click)="openForm()">
          <mat-icon>add</mat-icon> New Order
        </button>
      </div>

      <mat-card class="filter-card">
        <mat-card-content>
          <form [formGroup]="filterForm" class="filter-row">
            <mat-form-field>
              <mat-label>Status</mat-label>
              <mat-select formControlName="status">
                <mat-option [value]="null">All</mat-option>
                <mat-option [value]="1">Pending</mat-option>
                <mat-option [value]="2">In Progress</mat-option>
                <mat-option [value]="3">Completed</mat-option>
              </mat-select>
            </mat-form-field>
            <mat-form-field>
              <mat-label>Client Name</mat-label>
              <input matInput formControlName="clientName" />
            </mat-form-field>
            <mat-form-field>
              <mat-label>Client Document</mat-label>
              <input matInput formControlName="clientDocument" />
            </mat-form-field>
            <mat-form-field>
              <mat-label>Technician</mat-label>
              <input matInput formControlName="technicianName" />
            </mat-form-field>
            <mat-form-field>
              <mat-label>Specialty</mat-label>
              <input matInput formControlName="specialty" />
            </mat-form-field>
            <div style="display:flex;align-items:center;gap:8px">
              <button mat-flat-button color="accent" type="button" (click)="applyFilter()">
                <mat-icon>search</mat-icon> Search
              </button>
              <button mat-button type="button" (click)="clearFilter()">
                <mat-icon>clear</mat-icon> Clear
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>

      <mat-card *ngIf="loading">
        <mat-card-content style="text-align:center;padding:32px">
          <mat-spinner diameter="40" style="margin:auto"></mat-spinner>
        </mat-card-content>
      </mat-card>

      <mat-card *ngIf="!loading">
        <mat-card-content>
          <table mat-table [dataSource]="orders" class="full-width">
            <ng-container matColumnDef="clientName">
              <th mat-header-cell *matHeaderCellDef>Client</th>
              <td mat-cell *matCellDef="let o">
                <div>{{ o.clientName }}</div>
                <small style="color:#888">{{ o.clientDocument }}</small>
              </td>
            </ng-container>
            <ng-container matColumnDef="technicianName">
              <th mat-header-cell *matHeaderCellDef>Technician</th>
              <td mat-cell *matCellDef="let o">
                <div>{{ o.technicianName }}</div>
                <small style="color:#888">{{ o.specialty }}</small>
              </td>
            </ng-container>
            <ng-container matColumnDef="description">
              <th mat-header-cell *matHeaderCellDef>Description</th>
              <td mat-cell *matCellDef="let o">{{ o.description }}</td>
            </ng-container>
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let o">
                <span class="status-badge"
                  [ngClass]="{
                    'status-pending': o.status === 1,
                    'status-inprogress': o.status === 2,
                    'status-completed': o.status === 3
                  }">
                  {{ o.statusName }}
                </span>
              </td>
            </ng-container>
            <ng-container matColumnDef="createdAt">
              <th mat-header-cell *matHeaderCellDef>Created</th>
              <td mat-cell *matCellDef="let o">{{ o.createdAt | date:'dd/MM/yyyy' }}</td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let o">
                <button mat-icon-button color="accent" (click)="changeStatus(o)" matTooltip="Change Status">
                  <mat-icon>sync</mat-icon>
                </button>
                <button mat-icon-button color="primary" (click)="openForm(o)" matTooltip="Edit">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button color="warn" (click)="deleteOrder(o)" matTooltip="Delete">
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
  `,
  styles: ['.filter-card { margin-bottom: 16px; }']
})
export class OrderListComponent implements OnInit {
  private orderService = inject(OrderService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  orders: Order[] = [];
  loading = false;
  displayedColumns = ['clientName', 'technicianName', 'description', 'status', 'createdAt', 'actions'];

  filterForm = this.fb.group({
    status: [null as number | null],
    clientName: [''],
    clientDocument: [''],
    technicianName: [''],
    specialty: ['']
  });

  ngOnInit(): void { this.load(); }

  load(filter?: OrderFilter): void {
    this.loading = true;
    this.orderService.getAll(filter).subscribe({
      next: orders => { this.orders = orders; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  applyFilter(): void {
    const v = this.filterForm.value;
    const filter: OrderFilter = {
      status: v.status ?? undefined,
      clientName: v.clientName || undefined,
      clientDocument: v.clientDocument || undefined,
      technicianName: v.technicianName || undefined,
      specialty: v.specialty || undefined
    };
    this.load(filter);
  }

  clearFilter(): void {
    this.filterForm.reset();
    this.load();
  }

  openForm(order?: Order): void {
    const ref = this.dialog.open(OrderFormComponent, { width: '580px', data: { order } });
    ref.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open(order ? 'Order updated.' : 'Order created.', 'Close', { duration: 3000 });
        this.load();
      }
    });
  }

  changeStatus(order: Order): void {
    const ref = this.dialog.open(OrderStatusComponent, { width: '400px', data: order });
    ref.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Status updated.', 'Close', { duration: 3000 });
        this.load();
      }
    });
  }

  deleteOrder(order: Order): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete Order', message: `Delete order for ${order.clientName}?` }
    });
    ref.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.orderService.delete(order.id).subscribe({
          next: () => { this.snackBar.open('Order deleted.', 'Close', { duration: 3000 }); this.load(); },
          error: err => this.snackBar.open(err.error?.message ?? 'Error.', 'Close', { duration: 4000 })
        });
      }
    });
  }
}
