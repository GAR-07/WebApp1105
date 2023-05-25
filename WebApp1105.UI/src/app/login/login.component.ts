import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { LoginAccount } from 'src/models/loginAccount.model';
import { AuthService } from 'src/app/_services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  accountLoginRequest: LoginAccount = {
    userName: 'Александр',
    password: '1234',
    typeAuth: 'Cookie'
  }
  constructor(
    private authService: AuthService, 
    private router: Router
    ) { }

  onSubmit() {
    this.authService.login(this.accountLoginRequest)
    .subscribe({
      next: (response: any) => {
      console.log(response);
      if (response.access_token)
      {
        localStorage.setItem('access_token', response.access_token);
      }
      localStorage.setItem('isLoggedIn', '+')
      this.router.navigate(['cabinet']);
    },
      error: (response) => {
        console.log(response);
      }
    });
  }
}