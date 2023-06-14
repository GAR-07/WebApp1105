import { Router } from '@angular/router';
import { Component } from '@angular/core';
import { AuthService } from 'src/app/_services/auth.service';
import { Account } from '../_interfaces/account.model';
import { StorageService } from '../_services/storage.service';
import { HttpEventType } from '@angular/common/http';
import { ImageToCreate } from '../_interfaces/FileToCreate.model';

@Component({
  selector: 'app-cabinet',
  templateUrl: './cabinet.component.html',
  styleUrls: ['./cabinet.component.css']
})

export class CabinetComponent {

  user: Account = { 
    userId: null,
    userName: null
  };
  image: ImageToCreate = {
    userId: '',
    title: null,
    description: null,
    fileType: ['', ''],
    filePath: ''
  };
  title: string = '';
  description: string = '';
  response = {
    contentType: ['', ''],
    dbPath: ''
  };

  contentType: string = '';
  progress: number = 0;
  message: string = '';

  constructor(
    private authService: AuthService, 
    private storageService: StorageService,
    private router: Router
    ) { }

  ngOnInit(): void {
    this.authService.accountConfirm()
    .subscribe({
      next: (response: any) => {
        console.log(response);
        if (response.userName)
        {
          this.user.userId = response.userId;
          this.user.userName = response.userName;
        }
      },
      error: (response) => {
        console.log(response);
        this.router.navigate(['login']);
      }
    });
  }

  uploadFile = (files : any) => {
    if (files.length === 0 || this.user.userId === null) {
      return;
    }

    const formData = new FormData();

    var fileToUpload = <File>files[0];
    formData.append('files.Account', this.user.userId)
    formData.append('files.File', fileToUpload, fileToUpload.name)
    
    this.storageService.uploadFile(formData)
      .subscribe({
        next: (event: any) => {
        this.message = '';
        if (event.type === HttpEventType.UploadProgress)
          this.progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          this.message = 'Загрузка завершена';
          this.response = event.body;
        }
      },
      error: (response) => console.log(response)
    });

  }

  onCreate = () => {
    this.image = {
      userId: this.user.userId,
      title: this.title,
      description: this.description,
      filePath: this.response.dbPath,
      fileType: this.response.contentType
    }
    this.message = 'Идёт обработка...';
    this.storageService.saveFile(this.image)
    .subscribe({
      next: (response:any) => {
        console.log(response);
        this.message = 'Сохранено';
      },
      error: (response:any) => console.log(response)
    });
  }

  createImgPath = (serverPath: string) => { 
    return `https://localhost:7185/${serverPath}`; 
  }

  goToLink(serverPath: string){
    window.open(`https://localhost:7185/${serverPath}`, "_blank");
  }
}