import { Subscription, SubscriptionPlan } from '../../models/subscription.model';

export interface SubscriptionState {
  currentSubscription: Subscription | null;
  subscriptionPlans: SubscriptionPlan[];
  loading: boolean;
  error: string | null;
}

export const initialSubscriptionState: SubscriptionState = {
  currentSubscription: null,
  subscriptionPlans: [],
  loading: false,
  error: null
};
