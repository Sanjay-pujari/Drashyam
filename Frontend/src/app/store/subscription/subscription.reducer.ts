import { createReducer, on } from '@ngrx/store';
import { SubscriptionState, initialSubscriptionState } from './subscription.state';
import * as SubscriptionActions from './subscription.actions';

export const subscriptionReducer = createReducer(
  initialSubscriptionState,
  
  on(SubscriptionActions.loadSubscriptionPlans, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(SubscriptionActions.loadSubscriptionPlansSuccess, (state, { plans }) => ({
    ...state,
    subscriptionPlans: plans,
    loading: false,
    error: null
  })),
  
  on(SubscriptionActions.loadSubscriptionPlansFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
