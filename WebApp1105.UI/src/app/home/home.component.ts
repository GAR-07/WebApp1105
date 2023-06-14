import { Component } from '@angular/core';
import { StorageService } from '../_services/storage.service';
import { Image } from '../_interfaces/image.model';
import { Video } from '../_interfaces/video.model';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {

  images: Image[] = [];
  videos: Video[] = [];

  constructor(
    private storageService: StorageService,
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
    this.storageService.getAllVideo()
    .subscribe({
      next: (response: any) => {
        if (response)
        {
          for (var i = 0; i < response.length; i++)
          {
            this.videos[i] = response[i];
          }
        }
      },
      error: (response) => console.log(response)
    });
  }

  createFilePath = (serverPath: string) => { 
    return `https://localhost:7185/${serverPath}`; 
  }

  goToLink(serverPath: string){
    window.open(`https://localhost:7185/${serverPath}`, "_blank");
  }
}
