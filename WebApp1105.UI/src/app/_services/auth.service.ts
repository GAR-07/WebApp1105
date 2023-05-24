import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { environment } from 'src/environments/environment';
import { Account } from '../../models/account.model';
import { LoginAccount } from '../../models/loginAccount.model';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})

export class AuthService {

  baseApiUrl: string = environment.baseApiUrl;

  constructor(    
    private http: HttpClient,
    private router: Router,
    ) { }

  login(accountLoginRequest : LoginAccount) {
    return this.http.post(this.baseApiUrl + '/Account/Login', accountLoginRequest,)
    }

  logout() {
    this.http.get<Account>(this.baseApiUrl + '/Account/Logout')
    .subscribe({
      next: (response: any) => {
        console.log(response);
      },
      error: (response) => {
        console.log(response);
      }
    });
    localStorage.clear();
    sessionStorage.clear();
    console.log('Произведён выход из аккаунта');
    this.router.navigate(['login'])
    }

  cabinet(headers : HttpHeaders) {
    return this.http.get<Account>(this.baseApiUrl + '/Account/Cabinet', { headers: headers })
  }
}