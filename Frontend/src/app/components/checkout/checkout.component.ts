import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatStepperModule } from '@angular/material/stepper';
import { MatRadioModule } from '@angular/material/radio';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { CartService, Cart, CartItem } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatStepperModule,
    MatRadioModule,
    MatProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss']
})
export class CheckoutComponent implements OnInit, OnDestroy {
  cart: Cart = { items: [], totalItems: 0, totalPrice: 0, currency: 'USD' };
  private cartSubscription?: Subscription;
  
  // Form groups
  shippingForm: FormGroup;
  paymentForm: FormGroup;
  
  // Payment options
  paymentMethods = [
    { value: 'card', label: 'Credit/Debit Card', icon: 'credit_card' },
    { value: 'paypal', label: 'PayPal', icon: 'account_balance' },
    { value: 'stripe', label: 'Stripe', icon: 'payment' }
  ];
  
  selectedPaymentMethod = 'card';
  isProcessing = false;

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar,
    private fb: FormBuilder
  ) {
    this.shippingForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      address: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      zipCode: ['', Validators.required],
      country: ['', Validators.required]
    });

    this.paymentForm = this.fb.group({
      cardNumber: ['', Validators.required],
      expiryDate: ['', Validators.required],
      cvv: ['', Validators.required],
      cardName: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.cartSubscription = this.cartService.cart$.subscribe(cart => {
      this.cart = cart;
    });

    // Redirect if cart is empty
    if (this.cart.items.length === 0) {
      this.router.navigate(['/cart']);
    }

    // Pre-fill email if user is logged in
    if (this.authService.isAuthenticated()) {
      this.authService.getCurrentUser().subscribe(user => {
        if (user) {
          this.shippingForm.patchValue({
            email: user.email,
            firstName: user.firstName,
            lastName: user.lastName
          });
        }
      });
    }
  }

  ngOnDestroy() {
    this.cartSubscription?.unsubscribe();
  }

  getItemTotal(item: CartItem): number {
    return item.price * item.quantity;
  }

  placeOrder() {
    if (this.shippingForm.invalid) {
      this.snackBar.open('Please fill in all shipping details', 'Close', {
        duration: 3000
      });
      return;
    }

    if (this.selectedPaymentMethod === 'card' && this.paymentForm.invalid) {
      this.snackBar.open('Please fill in all payment details', 'Close', {
        duration: 3000
      });
      return;
    }

    this.isProcessing = true;

    // Simulate order processing
    setTimeout(() => {
      this.isProcessing = false;
      this.snackBar.open('Order placed successfully!', 'Close', {
        duration: 5000
      });
      
      // Clear cart and redirect
      this.cartService.clearCart();
      this.router.navigate(['/']);
    }, 2000);
  }

  goBack() {
    this.router.navigate(['/cart']);
  }
}
