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
    {
      // const accessToken = localStorage.getItem('accessToken');
      // if (accessToken)
      //   var headers = new HttpHeaders().set('Authorization', 'Bearer ' + accessToken);
      // else
      //   var headers = new HttpHeaders().set('Access-Control-Allow-Credentials', 'true');

      // this.authService.cabinet(headers)
      // .subscribe({
      //   next: (response: any) => {
      //     const userName = response.userName;
      //     if (userName)
            this.isLoggedIn = true
      //     else
      //       this.isLoggedIn = false
      //   },
      //   error: (response) => {
      //     console.log(response);
      //     this.router.navigate(['login']);
      //   }
      // });
    }
  }

  logout(): void {
    this.isLoggedIn = false;
      this.authService.logout().subscribe({
      next: response => {
        // console.log(res);
        localStorage.clear();
        sessionStorage.clear();
        this.router.navigate(['login']);
      },
      error: response => console.log(response)
    });
  }
}