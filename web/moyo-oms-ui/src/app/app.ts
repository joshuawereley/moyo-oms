import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  private readonly authService = inject(MsalService);
  private readonly broadcastService = inject(MsalBroadcastService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly isLoggedIn = signal(false);
  protected readonly userName = signal('');
  protected readonly navOpen = signal(false);

  ngOnInit(): void {
    this.authService.initialize().subscribe(() => {
      this.authService.handleRedirectObservable().subscribe((result) => {
        if (result?.account) {
          this.authService.instance.setActiveAccount(result.account);
        }
        this.ensureActiveAccount();
        this.updateLoginState();
      });
    });

    this.broadcastService.inProgress$
      .pipe(
        filter((status) => status === InteractionStatus.None),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.ensureActiveAccount();
        this.updateLoginState();
      });
  }

  protected login(): void {
    this.authService.loginRedirect();
  }

  protected logout(): void {
    this.authService.logoutRedirect();
  }

  protected toggleNav(): void {
    this.navOpen.update((open) => !open);
  }

  protected closeNav(): void {
    this.navOpen.set(false);
  }

  private ensureActiveAccount(): void {
    const active = this.authService.instance.getActiveAccount();
    const accounts = this.authService.instance.getAllAccounts();
    if (!active && accounts.length > 0) {
      this.authService.instance.setActiveAccount(accounts[0]);
    }
  }

  private updateLoginState(): void {
    const account = this.authService.instance.getActiveAccount();
    this.isLoggedIn.set(this.authService.instance.getAllAccounts().length > 0);
    this.userName.set(account?.name ?? account?.username ?? '');
  }
}
