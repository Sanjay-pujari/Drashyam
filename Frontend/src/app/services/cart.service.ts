import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface CartItem {
  id: number;
  name: string;
  price: number;
  currency: string;
  imageUrl?: string;
  quantity: number;
  channelId: number;
  channelName: string;
  sizes?: string[];
  colors?: string[];
  selectedSize?: string;
  selectedColor?: string;
}

export interface Cart {
  items: CartItem[];
  totalItems: number;
  totalPrice: number;
  currency: string;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartSubject = new BehaviorSubject<Cart>({
    items: [],
    totalItems: 0,
    totalPrice: 0,
    currency: 'USD'
  });

  public cart$ = this.cartSubject.asObservable();

  constructor() {
    // Load cart from localStorage on service initialization
    this.loadCartFromStorage();
  }

  private loadCartFromStorage(): void {
    try {
      const savedCart = localStorage.getItem('drashyam_cart');
      if (savedCart) {
        const cart = JSON.parse(savedCart);
        this.cartSubject.next(cart);
      }
    } catch (error) {
      console.error('Error loading cart from storage:', error);
    }
  }

  private saveCartToStorage(cart: Cart): void {
    try {
      localStorage.setItem('drashyam_cart', JSON.stringify(cart));
    } catch (error) {
      console.error('Error saving cart to storage:', error);
    }
  }

  private updateCart(): void {
    const currentCart = this.cartSubject.value;
    const totalItems = currentCart.items.reduce((sum, item) => sum + item.quantity, 0);
    const totalPrice = currentCart.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    
    const updatedCart: Cart = {
      ...currentCart,
      totalItems,
      totalPrice
    };

    this.cartSubject.next(updatedCart);
    this.saveCartToStorage(updatedCart);
  }

  addItem(item: Omit<CartItem, 'quantity'>): void {
    const currentCart = this.cartSubject.value;
    const existingItemIndex = currentCart.items.findIndex(
      cartItem => cartItem.id === item.id && 
      cartItem.selectedSize === item.selectedSize && 
      cartItem.selectedColor === item.selectedColor
    );

    if (existingItemIndex > -1) {
      // Update existing item quantity
      currentCart.items[existingItemIndex].quantity += 1;
    } else {
      // Add new item
      currentCart.items.push({ ...item, quantity: 1 });
    }

    this.updateCart();
  }

  removeItem(itemId: number, selectedSize?: string, selectedColor?: string): void {
    const currentCart = this.cartSubject.value;
    currentCart.items = currentCart.items.filter(
      item => !(item.id === itemId && 
      item.selectedSize === selectedSize && 
      item.selectedColor === selectedColor)
    );

    this.updateCart();
  }

  updateQuantity(itemId: number, quantity: number, selectedSize?: string, selectedColor?: string): void {
    if (quantity <= 0) {
      this.removeItem(itemId, selectedSize, selectedColor);
      return;
    }

    const currentCart = this.cartSubject.value;
    const itemIndex = currentCart.items.findIndex(
      item => item.id === itemId && 
      item.selectedSize === selectedSize && 
      item.selectedColor === selectedColor
    );

    if (itemIndex > -1) {
      currentCart.items[itemIndex].quantity = quantity;
      this.updateCart();
    }
  }

  clearCart(): void {
    this.cartSubject.next({
      items: [],
      totalItems: 0,
      totalPrice: 0,
      currency: 'USD'
    });
    this.saveCartToStorage(this.cartSubject.value);
  }

  getCart(): Cart {
    return this.cartSubject.value;
  }

  getItemCount(): number {
    return this.cartSubject.value.totalItems;
  }

  getTotalPrice(): number {
    return this.cartSubject.value.totalPrice;
  }

  isItemInCart(itemId: number, selectedSize?: string, selectedColor?: string): boolean {
    return this.cartSubject.value.items.some(
      item => item.id === itemId && 
      item.selectedSize === selectedSize && 
      item.selectedColor === selectedColor
    );
  }
}
