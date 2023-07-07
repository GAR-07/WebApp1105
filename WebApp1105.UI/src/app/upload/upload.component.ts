import { Component } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AppComponent } from '../app.component';
import { Account } from '../_interfaces/account.model';


@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})
export class UploadComponent {

  mode: string = 'create';
  user: Account = { 
    userId: null,
    userName: null
  };

  constructor(
    private readonly authService: AuthService, 
    private readonly appComponent: AppComponent,
    ) { }

  ngOnInit(): void {
    this.authService.accountConfirm()
    .subscribe({
      next: (response: any) => {
        if (response.userName)
        {
          this.user.userId = response.userId;
          this.user.userName = response.userName;
        }
        //this.storageService.initialUser(this.user);
      },
      error: (response) => {
        console.log(response);
        this.appComponent.logout();
      }
    });
  }
}
