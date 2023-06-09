import { Component } from '@angular/core';
import { StorageService } from '../_services/storage.service';
import { Router } from '@angular/router';
import { Image } from '../_interfaces/image.model';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {

  images: Image[] = [];

  constructor(
    private storageService: StorageService,
    private router: Router
    ) { }

  ngOnInit(): void {
    this.storageService.getAllImage()
    .subscribe({
      next: (response: any) => {
        if (response)
        {
          for (var i = 0; i < response.length; i++)
          {
            this.images[i] = response[i];
          }
        }
      },
      error: (response) => console.log(response)
    });
    console.log('hello!', this.images);
  }

  public createImgPath = (serverPath: string | null) => { 
    return `https://localhost:7185/${serverPath}`; 
  }

  GoToLink(serverPath: string){
    window.open(`https://localhost:7185/${serverPath}`, "_blank");
  }
}
