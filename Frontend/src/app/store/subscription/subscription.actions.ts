import { createAction, props } from '@ngrx/store';
import { SubscriptionPlan } from '../../models/subscription.model';

export const loadSubscriptionPlans = createAction(
  '[Subscription] Load Subscription Plans'
);

export const loadSubscriptionPlansSuccess = createAction(
  '[Subscription] Load Subscription Plans Success',
  props<{ plans: SubscriptionPlan[] }>()
);

export const loadSubscriptionPlansFailure = createAction(
  '[Subscription] Load Subscription Plans Failure',
  props<{ error: string }>()
);
