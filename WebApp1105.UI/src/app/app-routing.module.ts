import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CabinetComponent } from './cabinet/cabinet.component';
import { LoginComponent } from './login/login.component';
import { cabinetGuard } from './cabinet/cabinet.guard';

const routes: Routes = [
  {
    path: '',
    component: CabinetComponent,
    canActivate: [cabinetGuard],
  },
  {
    path: 'cabinet',
    component: CabinetComponent,
    canActivate: [cabinetGuard],
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
