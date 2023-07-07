import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { AuthService } from './auth.service';
import { FileToCreate } from '../_interfaces/fileToCreate.model';
import { Book } from '../_interfaces/book.model';
import { itemsRequest } from '../_interfaces/itemsRequest.model';


@Injectable({
  providedIn: 'root'
})
export class StorageService {
  baseApiUrl: string = environment.baseApiUrl;

  constructor(
    private readonly authService: AuthService,
    private http: HttpClient,
  ) { }

  // observer = new Subject();
  // public user$ = this.observer.asObservable();
  // public initialUser(user: Account) {
  //   this.observer.next(user);
  //    console.log('service moment: ' + user.userId);
  // }

  uploadFiles(formData : FormData) {
    var headers = this.authService.headersInit();
    return this.http.post(this.baseApiUrl + '/Storage/UploadFiles', formData, { 
        headers: headers, 
        reportProgress: true, 
        observe: 'events' 
      });
  }

  saveFiles(files : FileToCreate[]) {
    var headers = this.authService.headersInit();
    return this.http.post(this.baseApiUrl + '/Storage/SaveFiles', files, { headers: headers});
  }

  saveBook(book : Book) {
    var headers = this.authService.headersInit();
    return this.http.post(this.baseApiUrl + '/Storage/SaveBook', book, { headers: headers});
  }

  editBook(books : Book[]) {
    var headers = this.authService.headersInit();
    return this.http.post(this.baseApiUrl + '/Storage/EditBook', books, { headers: headers});
  }

  deleteBook(id : number) {
    var headers = this.authService.headersInit();
    return this.http.post(this.baseApiUrl + '/Storage/DeleteBook', id, { headers: headers});
  }

  deleteImage(id : number[]) {
    var headers = this.authService.headersInit();
    return this.http.post(this.baseApiUrl + '/Storage/DeleteImage', id, { headers: headers});
  }

  deleteVideo(id : number[]) {
    var headers = this.authService.headersInit();
    return this.http.post(this.baseApiUrl + '/Storage/DeleteVideo', id, { headers: headers});
  }

  getImages(data : itemsRequest) {
    if (data.userId === null)
      return this.http.post(this.baseApiUrl + '/Storage/GetImages', data);
    else {
      var headers = this.authService.headersInit();
      return this.http.post(this.baseApiUrl + '/Storage/GetPersonalImages', data, { headers: headers});
    }
  }

  getVideos(data : itemsRequest) {
    if (data.userId === null)
      return this.http.post(this.baseApiUrl + '/Storage/GetVideos', data);
    else {
      var headers = this.authService.headersInit();
      return this.http.post(this.baseApiUrl + '/Storage/GetPersonalVideos', data, { headers: headers});
    }
  }

  getBooks(data : itemsRequest) {
    if (data.userId === null)
      return this.http.post(this.baseApiUrl + '/Storage/GetBooks', data);
    else {
      var headers = this.authService.headersInit();
      return this.http.post(this.baseApiUrl + '/Storage/GetPersonalBooks', data, { headers: headers});
    }
  }
}
