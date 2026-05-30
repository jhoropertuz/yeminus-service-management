import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { NgIf } from '@angular/common';
import { Technician, CreateTechnicianRequest } from '../../../shared/models/technician.model';
import { TechnicianService } from '../../../core/services/technician.service';

export interface TechnicianFormData { technician?: Technician; }

@Component({
  selector: 'app-technician-form',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, NgIf],
  template: `
    <h2 mat-dialog-title>{{ data.technician ? 'Edit Technician' : 'New Technician' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
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
          <mat-label>Specialty</mat-label>
          <input matInput formControlName="specialty" />
          <mat-error *ngIf="form.get('specialty')?.hasError('required')">Required</mat-error>
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
export class TechnicianFormComponent {
  private fb = inject(FormBuilder);
  private technicianService = inject(TechnicianService);
  dialogRef = inject(MatDialogRef<TechnicianFormComponent>);
  data = inject<TechnicianFormData>(MAT_DIALOG_DATA);

  loading = false;
  error = '';

  form = this.fb.group({
    fullName: [this.data.technician?.fullName ?? '', Validators.required],
    documentNumber: [this.data.technician?.documentNumber ?? '', Validators.required],
    phone: [this.data.technician?.phone ?? '', Validators.required],
    email: [this.data.technician?.email ?? '', [Validators.required, Validators.email]],
    specialty: [this.data.technician?.specialty ?? '', Validators.required]
  });

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.error = '';
    const request = this.form.value as CreateTechnicianRequest;
    const op = this.data.technician
      ? this.technicianService.update(this.data.technician.id, request)
      : this.technicianService.create(request);

    op.subscribe({
      next: t => this.dialogRef.close(t),
      error: err => { this.error = err.error?.message ?? 'An error occurred.'; this.loading = false; }
    });
  }
}
