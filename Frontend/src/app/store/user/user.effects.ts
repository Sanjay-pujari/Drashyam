import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import * as UserActions from './user.actions';

@Injectable()
export class UserEffects {
  constructor(
    private actions$: Actions,
    private authService: AuthService
  ) {}

  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UserActions.login),
      switchMap(({ credentials }) =>
        this.authService.login(credentials).pipe(
          map(response => UserActions.loginSuccess({ 
            user: response.user, 
            token: response.token 
          })),
          catchError(error => of(UserActions.loginFailure({ error: error.message })))
        )
      )
    )
  );

  register$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UserActions.register),
      switchMap(({ userData }) =>
        this.authService.register(userData).pipe(
          map(response => UserActions.registerSuccess({ 
            user: response.user, 
            token: response.token 
          })),
          catchError(error => of(UserActions.registerFailure({ error: error.message })))
        )
      )
    )
  );

  loadCurrentUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UserActions.loadCurrentUser),
      switchMap(() =>
        this.authService.getCurrentUser().pipe(
          map(user => UserActions.loadCurrentUserSuccess({ user })),
          catchError(error => of(UserActions.loadCurrentUserFailure({ error: error.message })))
        )
      )
    )
  );
}
