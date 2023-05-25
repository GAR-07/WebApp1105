import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../_services/auth.service';

export const cabinetGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    return authService.isLoggedIn
};
