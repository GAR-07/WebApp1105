import { HttpEventType } from '@angular/common/http';
import { Component, HostListener, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Account } from 'src/app/_interfaces/account.model';
import { FileToCreate } from 'src/app/_interfaces/fileToCreate.model';
import { StorageService } from 'src/app/_services/storage.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-edit-file',
  templateUrl: './edit-file.component.html',
  styleUrls: ['./edit-file.component.css']
})
export class EditFileComponent {

  @Input() mode: string = '';
  @Input() user: Account = { 
    userId: null,
    userName: null
  };

  baseApiUrl: string = environment.baseApiUrl;
  file: FileToCreate = {
    userId: null,
    title: null,
    description: null,
    fileType: ['', ''],
    fileName: '',
    filePath: ''
  };
  files = [this.file];
  title: string = '';
  description: string = '';

  response = {
    contentType: [['', '']],
    fileName: [''],
    filePath: ['']
  };
  dragAreaClass: string = '';
  progress: number = 0;
  message: string = '';

  constructor(
    private readonly storageService: StorageService,
    private readonly router: Router,
    ) { }

  ngOnInit(): void {
    this.dragAreaClass = "dragarea";
  }

  uploadFile = (files : any) => {
    if (files.length === 0) {
      this.message = 'Файлы не обнаружены!';
      return;
    }
    if (this.user.userId === null) {
      this.message = 'Пользователь не авторизован!';
      return;
    }
    if (files.length > 10) {
      this.message = 'Нельзя загружать более 10 файлов за один раз!';
      return;
    }
    const formData = new FormData();
    formData.append('Account', this.user.userId)

    for(var i = 0; i < files.length; i++) {
      var fileToUpload = <File>files[i];
      if (fileToUpload.size > 100000000) {
        this.message = 'Файл '+ fileToUpload.name +' слишком большой!';
        return;
      }
      formData.append('files.File', fileToUpload, fileToUpload.name)
    }
    this.storageService.uploadFiles(formData)
    .subscribe({
      next: (event: any) => {
        this.message = '';
        if (event.type === HttpEventType.UploadProgress)
          this.progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          this.response = event.body;
          this.message = event.body.message;
        }
      },
      error: (response) => console.log(response)
    });
  }

  onSaveFile = () => {
    if (this.response.filePath[0] == '')
    {
      this.message = 'Выберите файл для сохранения!';
      return;
    }
    for(var i = 0; i < this.response.filePath.length; i++) {
      this.files[i] = 
        this.file = {
        userId: this.user.userId,
        title: this.title,
        description: this.description,
        fileName: this.response.fileName[i],
        filePath: this.response.filePath[i],
        fileType: this.response.contentType[i]
      }
    }
    this.message = 'Идёт обработка...';
    this.storageService.saveFiles(this.files)
    .subscribe({
      next: (response:any) => {
        console.log(response);
        this.message = response.message;
        this.router.navigate(['/cabinet']);
        response = {
          contentType: [['', '']],
          filePath: ['']
        };
      },
      error: (response:any) => console.log(response)
    });
  }

  createFilePath = (serverPath: string) => { 
    return `${this.baseApiUrl}/${serverPath}`; 
  }

  goToLink(serverPath: string){
    window.open(`${this.baseApiUrl}/${serverPath}`, "_blank");
  }

  @HostListener("dragover", ["$event"]) onDragOver(event: any) {
    this.dragAreaClass = "droparea";
    event.preventDefault();
  }
  @HostListener("dragenter", ["$event"]) onDragEnter(event: any) {
    this.dragAreaClass = "droparea";
    event.preventDefault();
  }
  @HostListener("dragend", ["$event"]) onDragEnd(event: any) {
    this.dragAreaClass = "dragarea";
    event.preventDefault();
  }
  @HostListener("dragleave", ["$event"]) onDragLeave(event: any) {
    this.dragAreaClass = "dragarea";
    event.preventDefault();
  }
  @HostListener("drop", ["$event"]) onDrop(event: any) {
    this.dragAreaClass = "dragarea";
    event.preventDefault();
    event.stopPropagation();
    if (event.dataTransfer.files) {
      let files: FileList = event.dataTransfer.files;
      this.uploadFile(files);
    }
  }
}
