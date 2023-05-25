import { Component } from '@angular/core';
import { Account } from 'src/models/account.model';
import { AuthService } from 'src/app/_services/auth.service';
import { HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cabinet',
  templateUrl: './cabinet.component.html',
  styleUrls: ['./cabinet.component.css']
})

export class CabinetComponent {

  user: Account = { userName: 'Гость'};

  constructor(
    private authService: AuthService, 
    private router: Router
    ) { }

  ngOnInit(): void {
    const accessToken = localStorage.getItem('access_token');
    if (accessToken)
      var headers = new HttpHeaders().set('Authorization', 'Bearer ' + accessToken);
    else
      var headers = new HttpHeaders().set('Access-Control-Allow-Credentials', 'true');

    this.authService.cabinet(headers)
    .subscribe({
      next: (response: any) => {
        //console.log(response);
        const userName = response.username;
        if (userName != 'Гость')
          this.user.userName = userName;
      },
      error: (response) => {
        console.log(response);
        this.router.navigate(['login']);
      }
    });
  }
}