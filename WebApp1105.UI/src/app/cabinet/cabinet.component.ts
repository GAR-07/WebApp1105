import { Component, ViewChild } from '@angular/core';
import { AuthService } from 'src/app/_services/auth.service';
import { Account } from '../_interfaces/account.model';
import { StorageService } from '../_services/storage.service';
import { environment } from 'src/environments/environment';
import { Image } from '../_interfaces/image.model';
import { Video } from '../_interfaces/video.model';
import { Book } from '../_interfaces/book.model';
import { YesNoModalComponent } from '../yes-no-modal/yes-no-modal.component';
import { itemsRequest } from '../_interfaces/itemsRequest.model';

@Component({
  selector: 'app-cabinet',
  templateUrl: './cabinet.component.html',
  styleUrls: ['./cabinet.component.css']
})

export class CabinetComponent {

  @ViewChild('deleteModal') public deleteModal!: YesNoModalComponent;

  baseApiUrl: string = environment.baseApiUrl;
  mode: string = 'edit';
  user: Account = { 
    userId: null,
    userName: null
  };
  images: Image[] = [];
  videos: Video[] = [];
  books: Book[] = [];
  bookEditStatus: boolean[] = [];
  itemsRequest: itemsRequest = {
    userId: null,
    pageSize: 8,
    page: 1,
  };
  imagesPageNum: number = 1;
  videosPageNum: number = 1;
  booksPageNum: number = 1;

  constructor(
    private authService: AuthService, 
    private storageService: StorageService,
    ) { }

  ngOnInit(): void {
    this.authService.accountConfirm()
    .subscribe({
      next: (response: any) => {
        if (response.userName) {
          this.user.userId = response.userId;
          this.user.userName = response.userName;
          this.getImages(1);
          this.getVideos(1);
          this.getBooks(1);
        }
      },
      error: (response) => {
        console.log(response);
      }
    });
  }

  getImages(pageNum : number) {
    this.images = [];
    this.imagesPageNum = pageNum;
    this.itemsRequest = {
      userId: this.user.userId,
      pageSize: 8,
      page: pageNum,
    };
    this.storageService.getImages(this.itemsRequest)
    .subscribe({
      next: (response: any) => {
        if (response) {
          for (var i = 0; i < response.length; i++) {
            if (response[i].userId === this.user.userId) {
              this.images.push(response[i]);
            }
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
      userId: this.user.userId,
      pageSize: 8,
      page: pageNum,
    };
    this.storageService.getVideos(this.itemsRequest)
    .subscribe({
      next: (response: any) => {
        if (response) {
          for (var i = 0; i < response.length; i++) {
            if (response[i].userId === this.user.userId) {
              this.videos.push(response[i]);
            }
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
      userId: this.user.userId,
      pageSize: 2,
      page: pageNum,
    };
    this.storageService.getBooks(this.itemsRequest)
    .subscribe({
      next: (response: any) => {
        if (response) {
          for (var i = 0; i < response.length; i++) {
            if (response[i].userId === this.user.userId) {
              this.books.push(response[i]);
              this.bookEditStatus[response[i].id] = false;
            }
          }
        }
      },
      error: (response) => console.log(response)
    });
  }

  openEditWindow(id : number) {
    this.bookEditStatus[id] = !this.bookEditStatus[id];
  }

  deleteImage(id : number[]) {
    // const userAnswer = await this.deleteModal.showAsync();
    // if (userAnswer === 'Yes') {
    //   console.log('delete');
    this.storageService.deleteImage(id)
    .subscribe({
      next: () => {
        var index = this.images.findIndex(d => d.id === id);
        this.images.splice(index, 1);
      },
      error: (response:any) => console.log(response)
    });
    // }
  }

  deleteVideo(id : number[]) {
    // const userAnswer = await this.deleteModal.showAsync();
    // if (userAnswer === 'Yes') {
    //   console.log('delete');
    this.storageService.deleteVideo(id)
    .subscribe({
      next: () => {
        var index = this.videos.findIndex(d => d.id === id);
        this.videos.splice(index, 1);
      },
      error: (response:any) => console.log(response)
    });
    // }
  }

  deleteBook(id : number) {
    // const userAnswer = await this.deleteModal.showAsync();
    // if (userAnswer === 'Yes') {
    //   console.log('delete');
      this.storageService.deleteBook(id)
      .subscribe({
        next: () => {
          var index = this.books.findIndex(d => d.id === id);
          this.books.splice(index, 1);
        },
        error: (response:any) => console.log(response)
      });
    // }
  }

  createFilePath = (serverPath: string) => { 
    return `${this.baseApiUrl}/${serverPath}`; 
  }

  goToLink(serverPath: string){
    window.open(`${this.baseApiUrl}/${serverPath}`, "_blank");
  }
}