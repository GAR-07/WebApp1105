import { CanActivateFn } from '@angular/router';

export const cabinetGuard: CanActivateFn = () => {
    if (localStorage.getItem('isLoggedIn'))
        return true
    else
        return false
};
