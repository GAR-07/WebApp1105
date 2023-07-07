import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { CabinetComponent } from './cabinet/cabinet.component';
import { UploadComponent } from './upload/upload.component';
import { EditBookComponent } from './edit-book/edit-book.component';
import { EditFileComponent } from './edit-file/edit-file.component';
import { YesNoModalComponent } from './yes-no-modal/yes-no-modal.component';

@NgModule({
  declarations: [
    AppComponent,
    CabinetComponent,
    LoginComponent,
    HomeComponent,
    UploadComponent,
    EditBookComponent,
    EditFileComponent,
    YesNoModalComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    MatProgressBarModule,
    BrowserAnimationsModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})

export class AppModule { 
}
