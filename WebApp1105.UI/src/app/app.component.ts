import { Component } from '@angular/core';
import { AuthService } from './_services/auth.service';
import { Router } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';

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
    if (localStorage.getItem('isLoggedIn') == '+')
      this.isLoggedIn = true
  }

  logout(): void {
    this.isLoggedIn = false;
    const accessToken = localStorage.getItem('access_token');
    if (accessToken)
      var headers = new HttpHeaders().set('Authorization', 'Bearer ' + accessToken);
    else
      var headers = new HttpHeaders().set('Access-Control-Allow-Credentials', 'true');
    this.authService.logout(headers).subscribe({
      next: res => {
        console.log(res);
        localStorage.clear();
        sessionStorage.clear();
        this.router.navigate(['login']);
        console.log('Произведён выход из аккаунта');
      },
      error: err => {
        console.log(err);
      }
    });
  }
}