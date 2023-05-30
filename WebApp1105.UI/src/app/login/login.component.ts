import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { LoginAccount } from 'src/models/loginAccount.model';
import { AuthService } from 'src/app/_services/auth.service';
import { AppComponent } from '../app.component';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  accountLoginRequest: LoginAccount = {
    userName: '',
    password: '',
    typeAuth: 'Cookie'
  }
  constructor(
    private authService: AuthService,
    private appComponent: AppComponent,
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
      this.appComponent.isLoggedIn = true;
      this.router.navigate(['cabinet']);
    },
      error: (response) => {
        console.log(response);
      }
    });
  }
}