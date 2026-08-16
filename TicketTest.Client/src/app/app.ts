import { Component } from '@angular/core';
import { TicketList } from './tickets/ticket-list/ticket-list';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    TicketList
  ],
  template: `
    <app-ticket-list />
  `
})
export class App {
}