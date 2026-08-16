import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  CreateTicketRequest,
  PagedResponse,
  Ticket,
  TicketQuery,
  UpdateTicketRequest
} from '../models/ticket.models';

@Injectable({
  providedIn: 'root'
})
export class TicketApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/tickets`;

  getTickets(query: TicketQuery): Observable<PagedResponse<Ticket>> {
    let params = new HttpParams();

    if (query.status) {
      params = params.set('status', query.status);
    }

    if (query.priority) {
      params = params.set('priority', query.priority);
    }

    if (query.assignedTo) {
      params = params.set('assignedTo', query.assignedTo);
    }

    if (query.search) {
      params = params.set('search', query.search);
    }

    if (query.page) {
      params = params.set('page', query.page);
    }

    if (query.pageSize) {
      params = params.set('pageSize', query.pageSize);
    }

    if (query.sortBy) {
      params = params.set('sortBy', query.sortBy);
    }

    if (query.sortDirection) {
      params = params.set('sortDirection', query.sortDirection);
    }

    return this.http.get<PagedResponse<Ticket>>(
      this.baseUrl,
      { params }
    );
  }

  getTicket(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(
      `${this.baseUrl}/${id}`
    );
  }

  createTicket(
    request: CreateTicketRequest
  ): Observable<Ticket> {
    return this.http.post<Ticket>(
      this.baseUrl,
      request
    );
  }

  updateTicket(
    id: number,
    request: UpdateTicketRequest
  ): Observable<Ticket> {
    return this.http.put<Ticket>(
      `${this.baseUrl}/${id}`,
      request
    );
  }

  deleteTicket(
    id: number,
    version: number
  ): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/${id}`,
      {
        params: {
          version: version.toString()
        }
      }
    );
  }
}