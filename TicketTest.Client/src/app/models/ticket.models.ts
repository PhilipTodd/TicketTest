export interface Ticket {
  id: number;
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  assignedTo: string | null;
  createdAt: string;
  updatedAt: string | null;
  version: number;
}

export type TicketStatus =
  | 'Open'
  | 'InProgress'
  | 'Resolved'
  | 'Closed';

export type TicketPriority =
  | 'Low'
  | 'Medium'
  | 'High'
  | 'Critical';

export interface CreateTicketRequest {
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  assignedTo: string | null;
}

export interface UpdateTicketRequest {
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  assignedTo: string | null;
  version: number;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface TicketQuery {
  status?: string;
  priority?: string;
  assignedTo?: string;
  search?: string;
  page?: number;
  pageSize?: number;
  sortBy?: 'CreatedAt' | 'Priority' | 'Title';
  sortDirection?: 'asc' | 'desc';
}