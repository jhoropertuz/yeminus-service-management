import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AsyncPipe } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet, RouterLink, RouterLinkActive,
    MatToolbarModule, MatSidenavModule, MatListModule,
    MatIconModule, MatButtonModule, MatTooltipModule, AsyncPipe
  ],
  template: `
    <mat-sidenav-container class="shell">
      <!-- ── Sidebar ────────────────────────────────────────── -->
      <mat-sidenav mode="side" opened class="sidebar">
        <div class="brand">
          <mat-icon class="brand-icon">settings_suggest</mat-icon>
          <span class="brand-name">Yeminus</span>
        </div>

        <mat-nav-list class="nav-list">
          <a mat-list-item routerLink="/orders" routerLinkActive="nav-active"
             matTooltip="Service Orders" matTooltipPosition="right">
            <mat-icon matListItemIcon>assignment</mat-icon>
            <span matListItemTitle>Orders</span>
          </a>
          <a mat-list-item routerLink="/clients" routerLinkActive="nav-active"
             matTooltip="Clients" matTooltipPosition="right">
            <mat-icon matListItemIcon>people</mat-icon>
            <span matListItemTitle>Clients</span>
          </a>
          <a mat-list-item routerLink="/technicians" routerLinkActive="nav-active"
             matTooltip="Technicians" matTooltipPosition="right">
            <mat-icon matListItemIcon>engineering</mat-icon>
            <span matListItemTitle>Technicians</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>

      <!-- ── Main content ──────────────────────────────────── -->
      <mat-sidenav-content class="main">
        <mat-toolbar color="primary" class="topbar">
          <span class="spacer"></span>
          <mat-icon style="margin-right:8px;opacity:.8">account_circle</mat-icon>
          <span class="user-name">{{ (authService.currentUser$ | async)?.fullName }}</span>
          <button mat-icon-button (click)="logout()" matTooltip="Logout" style="margin-left:8px">
            <mat-icon>logout</mat-icon>
          </button>
        </mat-toolbar>

        <div class="page-area">
          <router-outlet />
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    /* Shell */
    .shell { height: 100vh; }

    /* Sidebar */
    .sidebar {
      width: 230px;
      background: #1a237e;
      border-right: none;
      display: flex;
      flex-direction: column;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 18px 16px;
      color: white;
      border-bottom: 1px solid rgba(255,255,255,.12);
    }
    .brand-icon { font-size: 28px; width: 28px; height: 28px; }
    .brand-name { font-size: 1.25rem; font-weight: 600; letter-spacing: .5px; }

    .nav-list { padding-top: 8px; }

    /* Nav items */
    ::ng-deep .sidebar .mdc-list-item {
      color: rgba(255,255,255,.75) !important;
      border-radius: 0 24px 24px 0 !important;
      margin-right: 12px !important;
      margin-bottom: 2px !important;
    }
    ::ng-deep .sidebar .mdc-list-item:hover {
      background: rgba(255,255,255,.1) !important;
      color: white !important;
    }
    ::ng-deep .sidebar .mat-icon { color: rgba(255,255,255,.75) !important; }

    /* Active nav item */
    ::ng-deep .sidebar .nav-active {
      background: rgba(255,255,255,.18) !important;
      color: white !important;
    }
    ::ng-deep .sidebar .nav-active .mat-icon { color: white !important; }

    /* Topbar */
    .topbar {
      position: sticky;
      top: 0;
      z-index: 100;
      box-shadow: 0 2px 6px rgba(0,0,0,.2);
    }
    .user-name {
      font-size: .9rem;
      font-weight: 500;
      opacity: .9;
    }

    /* Page area */
    .page-area {
      background: #f5f5f5;
      min-height: calc(100vh - 64px);
    }

    .spacer { flex: 1 1 auto; }
  `]
})
export class LayoutComponent {
  authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
  }
}
