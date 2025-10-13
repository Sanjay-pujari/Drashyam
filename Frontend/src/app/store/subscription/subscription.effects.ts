import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { SubscriptionService } from '../../services/subscription.service';
import * as SubscriptionActions from './subscription.actions';

@Injectable()
export class SubscriptionEffects {
  constructor(
    private actions$: Actions,
    private subscriptionService: SubscriptionService
  ) {}

  loadSubscriptionPlans$ = createEffect(() =>
    this.actions$.pipe(
      ofType(SubscriptionActions.loadSubscriptionPlans),
      switchMap(() =>
        this.subscriptionService.getSubscriptionPlans().pipe(
          map(plans => SubscriptionActions.loadSubscriptionPlansSuccess({ plans })),
          catchError(error => of(SubscriptionActions.loadSubscriptionPlansFailure({ error: error.message })))
        )
      )
    )
  );
}
