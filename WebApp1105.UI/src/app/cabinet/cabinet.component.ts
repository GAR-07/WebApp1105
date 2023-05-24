import { Component } from '@angular/core';
import { Account } from 'src/models/account.model';
import { AuthService } from 'src/app/_services/auth.service';
import { HttpHeaders } from '@angular/common/http'

@Component({
  selector: 'app-cabinet',
  templateUrl: './cabinet.component.html',
  styleUrls: ['./cabinet.component.css']
})

export class CabinetComponent {

  user: Account = { userName: 'Гость'};

  constructor(private AuthService: AuthService) { }

  ngOnInit(): void {
    const accessToken = localStorage.getItem('access_token');
    if (accessToken)
    {
      var headers = new HttpHeaders().set('Authorization', 'Bearer ' + accessToken);
      this.AuthService.cabinet(headers)
      .subscribe({
        next: (response: any) => {
          console.log(response);
          const userName = response.username;
          if (userName)
            this.user.userName = userName;
        },
        error: (response) => {
          console.log(response);
        }
      });
    }
    else
    {
      var headers = new HttpHeaders().set('Access-Control-Allow-Credentials', 'true');
      this.AuthService.cabinet(headers)
      .subscribe({
        next: (response: any) => {
          console.log(response);
          const userName = response.username;
          if (userName)
            this.user.userName = userName;
        },
        error: (response) => {
          console.log(response);
        }
      });
    }
  }
}