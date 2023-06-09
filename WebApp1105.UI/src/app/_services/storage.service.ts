import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { AuthService } from './auth.service';
import { ImageToCreate } from '../_interfaces/imageToCreate.model';

@Injectable({
  providedIn: 'root'
})
export class StorageService {
  baseApiUrl: string = environment.baseApiUrl;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  uploadFile(formData : FormData) {
    var headers = this.authService.headersInit();
    return this.http.post(this.baseApiUrl + '/Storage/UploadFile', formData, { 
        headers: headers, 
        reportProgress: true, 
        observe: 'events' 
      });
  }

  saveImage(image : ImageToCreate) {
    var headers = this.authService.headersInit();
    return this.http.post(this.baseApiUrl + '/Storage/SaveImage', image, { headers: headers});
  }
  getAllImage() {
    return this.http.post(this.baseApiUrl + '/Storage/GetImage', { });
  }
}
