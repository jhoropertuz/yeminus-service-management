import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { NgIf } from '@angular/common';
import { Order, ChangeOrderStatusRequest, OrderStatus } from '../../../shared/models/order.model';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-order-status',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatSelectModule, MatButtonModule, NgIf],
  template: `
    <h2 mat-dialog-title>Change Order Status</h2>
    <mat-dialog-content>
      <p>Order: <strong>{{ data.description }}</strong></p>
      <form [formGroup]="form">
        <mat-form-field class="full-width">
          <mat-label>New Status</mat-label>
          <mat-select formControlName="status">
            <mat-option [value]="1">Pending</mat-option>
            <mat-option [value]="2">In Progress</mat-option>
            <mat-option [value]="3">Completed</mat-option>
          </mat-select>
        </mat-form-field>
        <p class="error-message" *ngIf="error">{{ error }}</p>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="accent" (click)="save()" [disabled]="loading">
        {{ loading ? 'Saving...' : 'Update Status' }}
      </button>
    </mat-dialog-actions>
  `
})
export class OrderStatusComponent {
  private fb = inject(FormBuilder);
  private orderService = inject(OrderService);
  dialogRef = inject(MatDialogRef<OrderStatusComponent>);
  data = inject<Order>(MAT_DIALOG_DATA);

  loading = false;
  error = '';

  form = this.fb.group({
    status: [this.data.status, Validators.required]
  });

  save(): void {
    this.loading = true;
    this.error = '';
    const request: ChangeOrderStatusRequest = { status: this.form.value.status! };

    this.orderService.changeStatus(this.data.id, request).subscribe({
      next: o => this.dialogRef.close(o),
      error: err => { this.error = err.error?.message ?? 'Error'; this.loading = false; }
    });
  }
}
