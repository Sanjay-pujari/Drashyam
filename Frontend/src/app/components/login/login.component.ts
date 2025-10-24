import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
	selector: 'app-login',
	standalone: true,
	imports: [CommonModule, ReactiveFormsModule],
	templateUrl: './login.component.html',
	styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
	form = this.fb.group({
		email: ['', [Validators.required, Validators.email]],
		password: ['', [Validators.required]],
		rememberMe: [false]
	});

	showPassword = false;
	isLoading = false;

	constructor(
		private fb: FormBuilder, 
		private auth: AuthService, 
		private router: Router,
		private http: HttpClient
	) {}

	ngOnInit() {
		// Test API connectivity
		this.testApiConnection();
	}

	testApiConnection() {
		console.log('Testing API connection...');
		this.http.get(`${environment.apiUrl}/api/health`).subscribe({
			next: (response) => {
				console.log('API connection successful:', response);
			},
			error: (err) => {
				console.error('API connection failed:', err);
				console.error('This means the backend is not running or not accessible on', environment.apiUrl);
			}
		});
	}

	submit() {
		if (this.form.invalid) {
			this.form.markAllAsTouched();
			console.warn('Login form invalid', this.form.value);
			return;
		}

		this.isLoading = true;
		const { email, password, rememberMe } = this.form.value as { email: string; password: string; rememberMe: boolean };
		
		this.auth.login({ email, password, rememberMe }).subscribe({
			next: () => {
				this.isLoading = false;
				this.router.navigateByUrl('/');
			},
			error: (err) => {
				this.isLoading = false;
				console.error('Login failed', err);
				console.error('Error details:', {
					status: err.status,
					statusText: err.statusText,
					message: err.message,
					error: err.error,
					url: err.url
				});
				
				// Show user-friendly error message
				if (err.status === 0) {
					alert('Unable to connect to the server. Please make sure the backend API is running on http://localhost:56380');
				} else if (err.status === 401) {
					alert('Invalid email or password. Please try again.');
				} else {
					alert('Login failed. Please try again.');
				}
			}
		});
	}

	togglePasswordVisibility() {
		this.showPassword = !this.showPassword;
	}
}



