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
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { CartService, Cart, CartItem } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-cart',
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
    FormsModule
  ],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent implements OnInit, OnDestroy {
  cart: Cart = { items: [], totalItems: 0, totalPrice: 0, currency: 'USD' };
  private cartSubscription?: Subscription;

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.cartSubscription = this.cartService.cart$.subscribe(cart => {
      this.cart = cart;
    });
  }

  ngOnDestroy() {
    this.cartSubscription?.unsubscribe();
  }

  updateQuantity(item: CartItem, quantity: number) {
    this.cartService.updateQuantity(
      item.id, 
      quantity, 
      item.selectedSize, 
      item.selectedColor
    );
  }

  removeItem(item: CartItem) {
    this.cartService.removeItem(
      item.id, 
      item.selectedSize, 
      item.selectedColor
    );
    this.snackBar.open(`${item.name} removed from cart`, 'Close', {
      duration: 3000
    });
  }

  clearCart() {
    this.cartService.clearCart();
    this.snackBar.open('Cart cleared', 'Close', {
      duration: 3000
    });
  }

  proceedToCheckout() {
    if (!this.authService.isAuthenticated()) {
      this.snackBar.open('Please login to proceed to checkout', 'Close', {
        duration: 3000
      });
      this.router.navigate(['/login']);
      return;
    }

    if (this.cart.items.length === 0) {
      this.snackBar.open('Your cart is empty', 'Close', {
        duration: 3000
      });
      return;
    }

    this.router.navigate(['/checkout']);
  }

  continueShopping() {
    this.router.navigate(['/channels']);
  }

  getItemTotal(item: CartItem): number {
    return item.price * item.quantity;
  }

  trackByItem(index: number, item: CartItem): string {
    return `${item.id}-${item.selectedSize || ''}-${item.selectedColor || ''}`;
  }
}
