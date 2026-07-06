import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  private readonly authService = inject(MsalService);
  private readonly broadcastService = inject(MsalBroadcastService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly isLoggedIn = signal(false);

  ngOnInit(): void {
    this.authService.initialize().subscribe(() => {
      this.authService.handleRedirectObservable().subscribe(() => this.updateLoginState());
    });

    this.broadcastService.inProgress$
      .pipe(
        filter((status) => status === InteractionStatus.None),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.updateLoginState());
  }

  protected login(): void {
    this.authService.loginRedirect();
  }

  protected logout(): void {
    this.authService.logoutRedirect();
  }

  private updateLoginState(): void {
    this.isLoggedIn.set(this.authService.instance.getAllAccounts().length > 0);
  }
}
