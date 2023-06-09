import { CanActivateFn } from '@angular/router';

export const CabinetGuard: CanActivateFn = () => {
    if (localStorage.getItem('isLoggedIn'))
        return true
    else
        return false
};
