import { Component } from '@angular/core';
import { AuthService } from './_services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'WebApp1105.UI';
  isLoggedIn = false;

constructor(
  private authService: AuthService,
  private router: Router
) {}

  ngOnInit(): void {
    this.authService.accountConfirm()
    .subscribe({
      next: (response: any) => {
        console.log(response);
        if (response.userName)
        {
          this.isLoggedIn = true
        }
      },
      error: (response) => {
        console.log(response);
      }
    });
  }

  logout(): void {
    this.isLoggedIn = false;
      this.authService.logout().subscribe({
      next: response => {
        // console.log(response);
        localStorage.removeItem('isLoggedIn');
        this.router.navigate(['login']);
      },
      error: response => console.log(response)
    });
  }
}