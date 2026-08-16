import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { TicketForm } from '../ticket-form/ticket-form';

import { TicketApiService } from '../../core/ticket-api.service';
import {
  Ticket,
  TicketPriority,
  TicketQuery,
  TicketStatus
} from '../../models/ticket.models';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTableModule
  ],
  templateUrl: './ticket-list.html',
  styleUrl: './ticket-list.scss'
})
export class TicketList implements OnInit {
  private readonly ticketApi = inject(TicketApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly displayedColumns = [
    'id',
    'title',
    'status',
    'priority',
    'assignedTo',
    'createdAt',
    'actions'
  ];

  readonly statuses: TicketStatus[] = [
    'Open',
    'InProgress',
    'Resolved',
    'Closed'
  ];

  readonly priorities: TicketPriority[] = [
    'Low',
    'Medium',
    'High',
    'Critical'
  ];

  
  status = '';
  priority = '';
  assignedTo = '';
  search = '';

  sortBy: 'CreatedAt' | 'Priority' | 'Title' = 'CreatedAt';
  sortDirection: 'asc' | 'desc' = 'desc';

  readonly tickets = signal<Ticket[]>([]);

  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);

  readonly isLoading = signal(false);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    const query: TicketQuery = {
      status: this.status || undefined,
      priority: this.priority || undefined,
      assignedTo: this.assignedTo.trim() || undefined,
      search: this.search.trim() || undefined,
      page: this.page(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    };

    this.ticketApi.getTickets(query).subscribe({
      next: response => {
        this.tickets.set(response.items);
        this.totalCount.set(response.totalCount);
        this.page.set(response.page);
        this.isLoading.set(false);
      },
      error: error => {
        console.error('Failed to load tickets', error);

        this.tickets.set([]);
        this.totalCount.set(0);
        this.errorMessage.set('Unable to load tickets.');
        this.isLoading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.page.set(1);
    this.loadTickets();
  }

  clearFilters(): void {
    this.status = '';
    this.priority = '';
    this.assignedTo = '';
    this.search = '';

    this.page.set(1);

    this.loadTickets();
  }

  pageChanged(event: PageEvent): void {
    this.page.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);

    this.loadTickets();
  }

  changeSort(field: 'CreatedAt' | 'Priority' | 'Title'): void {
    if (this.sortBy === field) {
      this.sortDirection =
        this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = field;
      this.sortDirection = field === 'Title' ? 'asc' : 'desc';
    }

    this.page.set(1);
    this.loadTickets();
  }

  sortIndicator(field: 'CreatedAt' | 'Priority' | 'Title'): string {
    if (this.sortBy !== field) {
      return '';
    }

    return this.sortDirection === 'asc' ? '▲' : '▼';
  }

  createTicket(): void {
    const dialogRef = this.dialog.open(TicketForm, {
      width: '600px',
      data: {}
    });

    dialogRef.afterClosed().subscribe(ticket => {
      if (ticket) {
        this.page.set(1);
        this.loadTickets();
      }
    });
  }

  editTicket(ticket: Ticket): void {
    if (ticket.status === 'Closed') {
      return;
    }

    const dialogRef = this.dialog.open(TicketForm, {
      width: '600px',
      data: {
        ticket
      }
    });

    dialogRef.afterClosed().subscribe(updatedTicket => {
      if (updatedTicket) {
        this.loadTickets();
      }
    });
  }

  deleteTicket(ticket: Ticket): void {
    if (ticket.status === 'Closed') {
      return;
    }

    const confirmed = window.confirm(
      `Delete ticket #${ticket.id} "${ticket.title}"?`
    );

    if (!confirmed) {
      return;
    }

    this.ticketApi.deleteTicket(
      ticket.id,
      ticket.version
    ).subscribe({
      next: () => {
        this.snackBar.open(
          `Ticket #${ticket.id} deleted.`,
          'Dismiss',
          {
            duration: 3000
          }
        );

        this.reconcileAfterDelete();
      },
      error: error => {
        console.error('Failed to delete ticket', error);
        this.handleDeleteError(error);
      }
    });
  }

  private handleDeleteError(error: any): void {
    let message: string;

    switch (error.status) {
      case 400:
        message =
          error?.error?.detail ??
          this.getValidationMessage(error) ??
          'The ticket cannot be deleted.';
        break;

      case 404:
        message =
          'The ticket no longer exists. The list will be refreshed.';
        break;

      case 409:
        message =
          'The ticket has been changed by another user. ' +
          'The list will be refreshed.';
        break;

      default:
        message =
          'Unable to delete the ticket.';
        break;
    }

    this.snackBar.open(
      message,
      'Dismiss',
      {
        duration: 5000
      }
    );

    if (error.status === 404 || error.status === 409) {
      this.loadTickets();
    }
  }  

  private getValidationMessage(error: any): string | null {
    const errors = error?.error?.errors;

    if (!errors) {
      return null;
    }

    const messages = Object.values(errors)
      .flatMap(value => value as string[]);

    return messages.length > 0
      ? messages.join(' ')
      : null;
  }  

  private reconcileAfterDelete(): void {
    const remainingItemsOnPage =
      this.tickets().length - 1;

    if (
      remainingItemsOnPage === 0 &&
      this.page() > 1
    ) {
      this.page.update(page => page - 1);
    }

    this.loadTickets();
  }
  
}
