import { createSelector } from '@ngrx/store';
import { AppState } from '../app.state';

export const selectUserState = (state: AppState) => state.user;

export const selectCurrentUser = createSelector(
  selectUserState,
  (state) => state.currentUser
);

export const selectIsAuthenticated = createSelector(
  selectUserState,
  (state) => state.isAuthenticated
);

export const selectUserToken = createSelector(
  selectUserState,
  (state) => state.token
);

export const selectUserLoading = createSelector(
  selectUserState,
  (state) => state.loading
);

export const selectUserError = createSelector(
  selectUserState,
  (state) => state.error
);
