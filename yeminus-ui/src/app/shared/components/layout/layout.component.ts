import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AsyncPipe } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet, RouterLink, RouterLinkActive,
    MatToolbarModule, MatSidenavModule, MatListModule,
    MatIconModule, MatButtonModule, AsyncPipe
  ],
  template: `
    <mat-sidenav-container class="sidenav-container">
      <mat-sidenav mode="side" opened class="sidenav">
        <mat-toolbar color="primary">
          <span>Yeminus</span>
        </mat-toolbar>
        <mat-nav-list>
          <a mat-list-item routerLink="/orders" routerLinkActive="active-link">
            <mat-icon matListItemIcon>assignment</mat-icon>
            <span matListItemTitle>Orders</span>
          </a>
          <a mat-list-item routerLink="/clients" routerLinkActive="active-link">
            <mat-icon matListItemIcon>people</mat-icon>
            <span matListItemTitle>Clients</span>
          </a>
          <a mat-list-item routerLink="/technicians" routerLinkActive="active-link">
            <mat-icon matListItemIcon>engineering</mat-icon>
            <span matListItemTitle>Technicians</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content>
        <mat-toolbar color="primary" class="top-toolbar">
          <span class="spacer"></span>
          <span>{{ (authService.currentUser$ | async)?.fullName }}</span>
          <button mat-icon-button (click)="logout()" title="Logout">
            <mat-icon>logout</mat-icon>
          </button>
        </mat-toolbar>
        <div class="content">
          <router-outlet />
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .sidenav-container { height: 100vh; }
    .sidenav { width: 220px; background: #fafafa; }
    .sidenav mat-toolbar { height: 56px; }
    .top-toolbar { position: sticky; top: 0; z-index: 10; }
    .content { padding: 24px; }
    .active-link { background: rgba(0,0,0,0.08); font-weight: 500; }
    .spacer { flex: 1 1 auto; }
  `]
})
export class LayoutComponent {
  authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
  }
}
