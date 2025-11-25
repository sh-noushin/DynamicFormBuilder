import { Component, signal } from '@angular/core';
import { NavigationEnd, Router, RouterModule, RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../header.component/header.component';
import { FooterComponent } from '../footer.component/footer.component';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-content',
  standalone: true,
  imports: [RouterOutlet, RouterModule, HeaderComponent, FooterComponent, MatSidenavModule, MatIconModule, MatButtonModule, MatListModule],
  templateUrl: './content.component.html',
  styleUrls: ['./content.component.scss']
})
export class ContentComponent {
  showSidenav = signal(false);

  constructor(private router: Router) { }

  ngOnInit(): void {
    const initialUrl = this.router.url || '/';
    this.showSidenav.set(initialUrl.startsWith('/admin'));

    this.router.events.subscribe(ev => {
      if (ev instanceof NavigationEnd) {
        const url = ev.urlAfterRedirects || ev.url;
        this.showSidenav.set(url.startsWith('/admin'));
      }
    });
  }
}