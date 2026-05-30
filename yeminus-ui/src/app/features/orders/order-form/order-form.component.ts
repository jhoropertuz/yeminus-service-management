import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { NgFor, NgIf } from '@angular/common';
import { Order, CreateOrderRequest, UpdateOrderRequest, OrderStatus } from '../../../shared/models/order.model';
import { Client } from '../../../shared/models/client.model';
import { Technician } from '../../../shared/models/technician.model';
import { OrderService } from '../../../core/services/order.service';
import { ClientService } from '../../../core/services/client.service';
import { TechnicianService } from '../../../core/services/technician.service';

export interface OrderFormData { order?: Order; }

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, NgFor, NgIf
  ],
  template: `
    <h2 mat-dialog-title>{{ data.order ? 'Edit Order' : 'New Order' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field class="full-width">
          <mat-label>Client</mat-label>
          <mat-select formControlName="clientId">
            <mat-option *ngFor="let c of clients" [value]="c.id">{{ c.fullName }}</mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('clientId')?.hasError('required')">Required</mat-error>
        </mat-form-field>

        <mat-form-field class="full-width">
          <mat-label>Technician</mat-label>
          <mat-select formControlName="technicianId">
            <mat-option *ngFor="let t of technicians" [value]="t.id">{{ t.fullName }} ({{ t.specialty }})</mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('technicianId')?.hasError('required')">Required</mat-error>
        </mat-form-field>

        <mat-form-field class="full-width" *ngIf="data.order">
          <mat-label>Status</mat-label>
          <mat-select formControlName="status">
            <mat-option [value]="1">Pending</mat-option>
            <mat-option [value]="2">In Progress</mat-option>
            <mat-option [value]="3">Completed</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="4"></textarea>
          <mat-error *ngIf="form.get('description')?.hasError('required')">Required</mat-error>
        </mat-form-field>

        <p class="error-message" *ngIf="error">{{ error }}</p>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" (click)="save()" [disabled]="loading">
        {{ loading ? 'Saving...' : 'Save' }}
      </button>
    </mat-dialog-actions>
  `
})
export class OrderFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private orderService = inject(OrderService);
  private clientService = inject(ClientService);
  private technicianService = inject(TechnicianService);
  dialogRef = inject(MatDialogRef<OrderFormComponent>);
  data = inject<OrderFormData>(MAT_DIALOG_DATA);

  clients: Client[] = [];
  technicians: Technician[] = [];
  loading = false;
  error = '';

  form = this.fb.group({
    clientId: [this.data.order?.clientId ?? '', Validators.required],
    technicianId: [this.data.order?.technicianId ?? '', Validators.required],
    status: [this.data.order?.status ?? OrderStatus.Pending],
    description: [this.data.order?.description ?? '', Validators.required]
  });

  ngOnInit(): void {
    this.clientService.getAll().subscribe(c => this.clients = c);
    this.technicianService.getAll().subscribe(t => this.technicians = t);
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.error = '';

    if (this.data.order) {
      const request: UpdateOrderRequest = {
        clientId: this.form.value.clientId!,
        technicianId: this.form.value.technicianId!,
        description: this.form.value.description!,
        status: this.form.value.status!
      };
      this.orderService.update(this.data.order.id, request).subscribe({
        next: o => this.dialogRef.close(o),
        error: err => { this.error = err.error?.message ?? 'Error'; this.loading = false; }
      });
    } else {
      const request: CreateOrderRequest = {
        clientId: this.form.value.clientId!,
        technicianId: this.form.value.technicianId!,
        description: this.form.value.description!
      };
      this.orderService.create(request).subscribe({
        next: o => this.dialogRef.close(o),
        error: err => { this.error = err.error?.message ?? 'Error'; this.loading = false; }
      });
    }
  }
}
