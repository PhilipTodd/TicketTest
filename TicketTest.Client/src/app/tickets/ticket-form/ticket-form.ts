import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

import { TicketApiService } from '../../core/ticket-api.service';
import {
  CreateTicketRequest,
  Ticket,
  TicketPriority,
  TicketStatus,
  UpdateTicketRequest
} from '../../models/ticket.models';

export interface TicketFormData {
  ticket?: Ticket;
}

@Component({
  selector: 'app-ticket-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule
  ],
  templateUrl: './ticket-form.html',
  styleUrl: './ticket-form.scss'
})
export class TicketForm {
  private readonly formBuilder = inject(FormBuilder);
  private readonly ticketApi = inject(TicketApiService);
  private readonly dialogRef = inject(MatDialogRef<TicketForm>);
  
  readonly data = inject<TicketFormData>(MAT_DIALOG_DATA);
  readonly isEditMode = !!this.data.ticket;

  readonly statuses: TicketStatus[] =
    this.getAllowedStatuses();

  readonly priorities: TicketPriority[] = [
    'Low',
    'Medium',
    'High',
    'Critical'
  ];

  readonly isSaving = signal(false);
  readonly serverError = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    title: [
      this.data.ticket?.title ?? '',
      [
        Validators.required,
        Validators.maxLength(150)
      ]
    ],
    description: [
      this.data.ticket?.description ?? '',
      Validators.required
    ],
    status: [
      this.data.ticket?.status ?? 'Open' as TicketStatus,
      Validators.required
    ],
    priority: [
      this.data.ticket?.priority ?? 'Medium' as TicketPriority,
      Validators.required
    ],
    assignedTo: [
      this.data.ticket?.assignedTo ?? ''
    ]
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    if (
      value.priority === 'Critical' &&
      !value.assignedTo.trim()
    ) {
      this.form.controls.assignedTo.setErrors({
        criticalRequiresAssignee: true
      });

      return;
    }

    this.isSaving.set(true);
    this.serverError.set('');

    if (this.isEditMode) {
      this.updateTicket(value);
    } else {
      this.createTicket(value);
    }
  }

  private createTicket(value: typeof this.form.value): void {
    const request: CreateTicketRequest = {
      title: value.title!.trim(),
      description: value.description!.trim(),
      status: value.status!,
      priority: value.priority!,
      assignedTo: value.assignedTo?.trim() || null
    };

    this.ticketApi.createTicket(request).subscribe({
      next: ticket => {
        this.isSaving.set(false);
        this.dialogRef.close(ticket);
      },
      error: error => {
        console.error('Failed to create ticket', error);

        this.isSaving.set(false);
        this.handleApiError(error);
      }
    });
  }

  private updateTicket(value: typeof this.form.value): void {
    const ticket = this.data.ticket!;

    const request: UpdateTicketRequest = {
      title: value.title!.trim(),
      description: value.description!.trim(),
      status: value.status!,
      priority: value.priority!,
      assignedTo: value.assignedTo?.trim() || null,
      version: ticket.version
    };

    this.ticketApi.updateTicket(
      ticket.id,
      request
    ).subscribe({
      next: updatedTicket => {
        this.isSaving.set(false);
        this.dialogRef.close(updatedTicket);
      },
      error: error => {
        console.error('Failed to update ticket', error);

        this.isSaving.set(false);
        this.handleApiError(error);
      }
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

private handleApiError(error: any): void {
    if (error.status === 409) {
      this.serverError.set(
        'This ticket has been changed by another user. ' +
        'Close this form and reload the ticket before trying again.'
      );

      return;
    }

    if (error.status === 404) {
      this.serverError.set(
        'This ticket no longer exists.'
      );

      return;
    }

    const errors = error?.error?.errors;

    if (errors) {
      for (const [field, messages] of Object.entries(errors)) {
        const controlName = this.toControlName(field);
        const control = this.form.get(controlName);

        if (control) {
          control.setErrors({
            server: (messages as string[]).join(' ')
          });
        }
      }

      return;
    }

    this.serverError.set(
      error?.error?.detail ??
      'Unable to save the ticket.'
    );
  }

  private toControlName(field: string): string {
    return field.charAt(0).toLowerCase() + field.slice(1);
  }

  private getAllowedStatuses(): TicketStatus[] {
    if (!this.data.ticket) {
      return ['Open', 'InProgress'];
    }

    switch (this.data.ticket.status) {
      case 'Open':
        return ['Open', 'InProgress', 'Resolved'];

      case 'InProgress':
        return ['InProgress', 'Open', 'Resolved'];

      case 'Resolved':
        return ['Resolved', 'InProgress', 'Closed'];

      case 'Closed':
        return ['Closed'];

      default:
        return [];
    }
  }
}