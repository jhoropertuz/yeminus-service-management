import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { NgIf } from '@angular/common';
import { Client, CreateClientRequest } from '../../../shared/models/client.model';
import { ClientService } from '../../../core/services/client.service';

export interface ClientFormData {
  client?: Client;
}

@Component({
  selector: 'app-client-form',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, NgIf],
  template: `
    <h2 mat-dialog-title>{{ data.client ? 'Edit Client' : 'New Client' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" id="clientForm">
        <mat-form-field class="full-width">
          <mat-label>Full Name</mat-label>
          <input matInput formControlName="fullName" />
          <mat-error *ngIf="form.get('fullName')?.hasError('required')">Required</mat-error>
        </mat-form-field>

        <mat-form-field class="full-width">
          <mat-label>Document Number</mat-label>
          <input matInput formControlName="documentNumber" />
          <mat-error *ngIf="form.get('documentNumber')?.hasError('required')">Required</mat-error>
        </mat-form-field>

        <mat-form-field class="full-width">
          <mat-label>Phone</mat-label>
          <input matInput formControlName="phone" />
          <mat-error *ngIf="form.get('phone')?.hasError('required')">Required</mat-error>
        </mat-form-field>

        <mat-form-field class="full-width">
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" type="email" />
          <mat-error *ngIf="form.get('email')?.hasError('required')">Required</mat-error>
          <mat-error *ngIf="form.get('email')?.hasError('email')">Invalid email</mat-error>
        </mat-form-field>

        <mat-form-field class="full-width">
          <mat-label>Address</mat-label>
          <textarea matInput formControlName="address" rows="2"></textarea>
          <mat-error *ngIf="form.get('address')?.hasError('required')">Required</mat-error>
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
export class ClientFormComponent {
  private fb = inject(FormBuilder);
  private clientService = inject(ClientService);
  dialogRef = inject(MatDialogRef<ClientFormComponent>);
  data = inject<ClientFormData>(MAT_DIALOG_DATA);

  loading = false;
  error = '';

  form = this.fb.group({
    fullName: [this.data.client?.fullName ?? '', Validators.required],
    documentNumber: [this.data.client?.documentNumber ?? '', Validators.required],
    phone: [this.data.client?.phone ?? '', Validators.required],
    email: [this.data.client?.email ?? '', [Validators.required, Validators.email]],
    address: [this.data.client?.address ?? '', Validators.required]
  });

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.loading = true;
    this.error = '';
    const request = this.form.value as CreateClientRequest;

    const op = this.data.client
      ? this.clientService.update(this.data.client.id, request)
      : this.clientService.create(request);

    op.subscribe({
      next: client => this.dialogRef.close(client),
      error: err => {
        this.error = err.error?.message ?? 'An error occurred.';
        this.loading = false;
      }
    });
  }
}
