import { Component } from '@angular/core';
import { StorageService } from '../_services/storage.service';
import { Image } from '../_interfaces/image.model';
import { Video } from '../_interfaces/video.model';
import { Book } from '../_interfaces/book.model';
import { environment } from 'src/environments/environment';
import { itemsRequest } from '../_interfaces/itemsRequest.model';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  baseApiUrl: string = environment.baseApiUrl;
  images: Image[] = [];
  videos: Video[] = [];
  books: Book[] = [];
  itemsRequest: itemsRequest = {
    userId: null,
    pageSize: 8,
    page: 1,
  };
  imagesPageNum: number = 1;
  videosPageNum: number = 1;
  booksPageNum: number = 1;

  constructor(
    private storageService: StorageService,
    ) { }

  ngOnInit(): void {
    this.getImages(1);
    this.getVideos(1);
    this.getBooks(1);
  }

  getImages(pageNum : number) {
    this.images = [];
    this.imagesPageNum = pageNum;
    this.itemsRequest = {
      userId: null,
      pageSize: 8,
      page: pageNum,
    };
    this.storageService.getImages(this.itemsRequest)
    .subscribe({
      next: (response: any) => {
        if (response) {
          for (var i = 0; i < response.length; i++) {
            this.images.push(response[i]);
          }
        }
      },
      error: (response) => console.log(response)
    });
  }

  getVideos(pageNum : number) {
    this.videos = [];
    this.videosPageNum = pageNum;
    this.itemsRequest = {
      userId: null,
      pageSize: 8,
      page: pageNum,
    };
    this.storageService.getVideos(this.itemsRequest)
    .subscribe({
      next: (response: any) => {
        if (response) {
          for (var i = 0; i < response.length; i++) {
            this.videos.push(response[i]);
          }
        }
      },
      error: (response) => console.log(response)
    });
  }

  getBooks(pageNum : number) {
    this.books = [];
    this.booksPageNum = pageNum;
    this.itemsRequest = {
      userId: null,
      pageSize: 2,
      page: pageNum,
    };
    this.storageService.getBooks(this.itemsRequest)
    .subscribe({
      next: (response: any) => {
        if (response) {
          for (var i = 0; i < response.length; i++) {
            this.books.push(response[i]);
          }
        }
      },
      error: (response) => console.log(response)
    });
  }

  createFilePath = (serverPath: string) => { 
    return `${this.baseApiUrl}/${serverPath}`; 
  }

  goToLink(serverPath: string){
    window.open(`${this.baseApiUrl}/${serverPath}`, "_blank");
  }
}
