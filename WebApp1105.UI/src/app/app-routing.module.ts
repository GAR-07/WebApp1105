import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CabinetComponent } from './cabinet/cabinet.component';
import { LoginComponent } from './login/login.component';

const routes: Routes = [
  {
    path: '',
    component: CabinetComponent
  },
  {
    path: 'cabinet',
    component: CabinetComponent
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
