import { HttpEventType } from '@angular/common/http';
import { Component, HostListener, Input } from '@angular/core';
import { StorageService } from '../_services/storage.service';
import { FileToCreate } from '../_interfaces/fileToCreate.model';
import { Account } from '../_interfaces/account.model';
import { Book } from '../_interfaces/book.model';
import { environment } from 'src/environments/environment';
import { Router } from '@angular/router';

@Component({
  selector: 'app-edit-book',
  templateUrl: './edit-book.component.html',
  styleUrls: ['./edit-book.component.css']
})
export class EditBookComponent {

  @Input() mode: string = '';
  @Input() user: Account = { 
    userId: null,
    userName: null
  };
  @Input() book!: Book;
  
  baseApiUrl: string = environment.baseApiUrl;
  title: string = '';
  description: string = '';
  genre: string = '';
  author: string = '';
  annotation: string | null = null;
  writingDate: number | null = null;
  publicationDate: number  | null = null;
  numberOfPages: number | null = null;
  publisher: string | null = null;
  file: FileToCreate = {
    userId: null,
    title: null,
    description: null,
    fileType: ['', ''],
    fileName: '',
    filePath: ''
  };
  files = [this.file];
  response = {
    contentType: [['', '']],
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
    if (this.mode == 'edit') {
      this.title = this.book.title;
      this.genre = this.book.genre;
      this.author = this.book.author;
      this.annotation = this.book.annotation;
      this.writingDate = this.book.writingDate;
      this.publicationDate = this.book.publicationDate;
      this.numberOfPages = this.book.numberOfPages;
      this.publisher = this.book.publisher;
    }
  }

  uploadBookFile = (files : any) => {
    if (this.user.userId === null) {
      this.message = 'Пользователь не авторизован!';
      return;
    }
    if (files.length != 1) {
      this.message = 'Можно загружать только по одному файлу!';
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
      formData.append('files.Book', fileToUpload, fileToUpload.name)
    }
    this.storageService.uploadFiles(formData)
    .subscribe({
      next: (event: any) => {
        this.message = '';
        if (event.type === HttpEventType.UploadProgress)
          this.progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          if (event.body.contentType[0][0] === 'image') {
            this.response.contentType[1] = event.body.contentType;
            this.response.filePath[1] = event.body.filePath;
          }
          else {
            this.response.contentType[0] = event.body.contentType;
            this.response.filePath[0] = event.body.filePath;
          }
          this.message = event.body.message;
        }
      },
      error: (response) => console.log(response)
    });
  }

  onSaveBook = () => {
    if (this.user.userId === null) {
      this.message = 'Пользователь не авторизован!';
      return;
    }
    if (this.response.filePath[0] == '') {
      this.message = 'Выберите файл для сохранения!';
      return;
    }
    var coverPath: string;
    if (this.response.filePath[1] == null) {
      coverPath = 'Resources\\Books\\DefaultBook.jpg';
    } else { coverPath = this.response.filePath[1].toString(); }
    var book: Book = {
    id: 0,
    userId: this.user.userId,
    title: this.title,
    genre: this.genre,
    author: this.author,
    annotation: this.annotation,
    bookFilePath: this.response.filePath[0].toString(),
    writingDate: this.writingDate ?? 0,
    publicationDate: this.publicationDate,
    coverPath: coverPath,
    numberOfPages: this.numberOfPages,
    publisher: this.publisher,
    }
    this.message = 'Идёт обработка...';
    this.storageService.saveBook(book)
    .subscribe({
      next: (response:any) => {
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

  onEditBook = () => {
    if (this.user.userId === null) {
      this.message = 'Пользователь не авторизован!';
      return;
    }
    var bookFilePath: string;
    if (this.response.filePath[0] != '') {
      bookFilePath = this.response.filePath[0].toString();
    } else { 
      bookFilePath = this.book.bookFilePath;
    }
    var coverPath: string;
    if (this.response.filePath[1] != null) {
      coverPath = this.response.filePath[1].toString();
    } else { 
      coverPath = this.book.coverPath; 
    }
    var books: Book[] = [
      this.book, {
      id: this.book.id,
      userId: this.user.userId,
      title: this.title,
      genre: this.genre,
      author: this.author,
      annotation: this.annotation,
      bookFilePath: bookFilePath,
      writingDate: this.writingDate ?? this.book.writingDate,
      publicationDate: this.publicationDate,
      coverPath: coverPath,
      numberOfPages: this.numberOfPages,
      publisher: this.publisher,
    }]
    this.message = 'Идёт обработка...';
    this.storageService.editBook(books)
    .subscribe({
      next: (response:any) => {
        console.log(response);
        this.message = response.message;
        // location.reload();
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
      this.uploadBookFile(files);
    }
  }
}
