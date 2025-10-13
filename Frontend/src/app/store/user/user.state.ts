import { User } from '../../models/user.model';

export interface UserState {
  currentUser: User | null;
  users: User[];
  loading: boolean;
  error: string | null;
  isAuthenticated: boolean;
  token: string | null;
}

export const initialUserState: UserState = {
  currentUser: null,
  users: [],
  loading: false,
  error: null,
  isAuthenticated: false,
  token: null
};
