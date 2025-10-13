import { createReducer, on } from '@ngrx/store';
import { UserState, initialUserState } from './user.state';
import * as UserActions from './user.actions';

export const userReducer = createReducer(
  initialUserState,
  
  on(UserActions.login, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(UserActions.loginSuccess, (state, { user, token }) => ({
    ...state,
    currentUser: user,
    token,
    isAuthenticated: true,
    loading: false,
    error: null
  })),
  
  on(UserActions.loginFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
    isAuthenticated: false,
    token: null
  })),

  on(UserActions.logout, (state) => ({
    ...state,
    currentUser: null,
    token: null,
    isAuthenticated: false
  })),

  on(UserActions.loadCurrentUser, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(UserActions.loadCurrentUserSuccess, (state, { user }) => ({
    ...state,
    currentUser: user,
    isAuthenticated: true,
    loading: false,
    error: null
  })),
  
  on(UserActions.loadCurrentUserFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
    isAuthenticated: false
  }))
);
