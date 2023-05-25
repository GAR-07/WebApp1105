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
  isLoggedIn = false;
  baseApiUrl: string = environment.baseApiUrl;

  constructor(
    private http: HttpClient,
    private router: Router,
    ) { }

  login(accountLoginRequest : LoginAccount) {
    return this.http.post(this.baseApiUrl + '/Account/Login', accountLoginRequest,)
  }

  cabinet(headers : HttpHeaders) {
    return this.http.get<Account>(this.baseApiUrl + '/Account/Cabinet', { headers: headers });
  }

  logout() {
    this.http.get<Account>(this.baseApiUrl + '/Account/Logout');
    localStorage.clear();
    sessionStorage.clear();
    this.isLoggedIn = false;
    console.log('isLoggedIn == false, ', this.isLoggedIn);
    console.log('Произведён выход из аккаунта');
    this.router.navigate(['login']);
  }


}