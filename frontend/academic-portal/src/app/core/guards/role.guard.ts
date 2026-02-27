import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const allowedRoles = (route.data['roles'] as string[] | undefined) ?? [];
  if (allowedRoles.length === 0) {
    return true;
  }

  if (auth.hasRole(allowedRoles)) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
