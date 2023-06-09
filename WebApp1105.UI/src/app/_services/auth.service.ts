import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { environment } from 'src/environments/environment';
import { LoginAccount } from '../_interfaces/loginAccount.model';
import { Account } from '../_interfaces/account.model';

@Injectable({
  providedIn: 'root'
})

export class AuthService {
  isLoggedIn = true;
  baseApiUrl: string = environment.baseApiUrl;

  constructor(
    private http: HttpClient,
    ) { }

  login(accountLoginRequest : LoginAccount) {
    return this.http.post(this.baseApiUrl + '/Account/Login', accountLoginRequest, )
  }

  accountConfirm() {
    var headers = this.headersInit();
    return this.http.get<Account>(this.baseApiUrl + '/Account/AccountConfirm', { headers: headers });
  }

  logout() {
    var headers = this.headersInit();
    return this.http.get<Account>(this.baseApiUrl + '/Account/Logout', { headers: headers });
  }

  headersInit(): HttpHeaders {
    const accessToken = localStorage.getItem('accessToken');
    if (accessToken)
      var headers = new HttpHeaders().set('Authorization', 'Bearer ' + accessToken);
    else
      var headers = new HttpHeaders();
      return headers
  }
}