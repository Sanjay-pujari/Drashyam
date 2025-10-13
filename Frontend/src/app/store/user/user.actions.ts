import { createAction, props } from '@ngrx/store';
import { User, UserLogin, UserRegistration } from '../../models/user.model';

export const login = createAction(
  '[User] Login',
  props<{ credentials: UserLogin }>()
);

export const loginSuccess = createAction(
  '[User] Login Success',
  props<{ user: User; token: string }>()
);

export const loginFailure = createAction(
  '[User] Login Failure',
  props<{ error: string }>()
);

export const register = createAction(
  '[User] Register',
  props<{ userData: UserRegistration }>()
);

export const registerSuccess = createAction(
  '[User] Register Success',
  props<{ user: User; token: string }>()
);

export const registerFailure = createAction(
  '[User] Register Failure',
  props<{ error: string }>()
);

export const logout = createAction(
  '[User] Logout'
);

export const loadCurrentUser = createAction(
  '[User] Load Current User'
);

export const loadCurrentUserSuccess = createAction(
  '[User] Load Current User Success',
  props<{ user: User }>()
);

export const loadCurrentUserFailure = createAction(
  '[User] Load Current User Failure',
  props<{ error: string }>()
);
