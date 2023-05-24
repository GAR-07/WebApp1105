import { Component } from '@angular/core';
import { AuthService } from './_services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'WebApp1105.UI';

constructor(
  private authService: AuthService,
) {}

logout() {
  this.authService.logout()
  }
}