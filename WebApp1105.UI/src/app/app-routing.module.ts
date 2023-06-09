import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CabinetComponent } from './cabinet/cabinet.component';
import { LoginComponent } from './login/login.component';
import { CabinetGuard } from './cabinet/cabinet.guard';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
  },
  {
    path: 'cabinet',
    component: CabinetComponent,
    canActivate: [CabinetGuard],
  },
  {
    path: 'login',
    component: LoginComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
