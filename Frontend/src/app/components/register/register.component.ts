import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
	selector: 'app-register',
	standalone: true,
	imports: [CommonModule, ReactiveFormsModule],
	templateUrl: './register.component.html',
	styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
	form = this.fb.group({
		firstName: ['', [Validators.required, Validators.maxLength(50)]],
		lastName: ['', [Validators.required, Validators.maxLength(50)]],
		email: ['', [Validators.required, Validators.email]],
		password: ['', [Validators.required, Validators.minLength(6)]],
		confirmPassword: ['', [Validators.required]]
	}, { validators: [this.passwordsMatchValidator] });

	isSubmitting = false;

	constructor(
		private fb: FormBuilder,
		private auth: AuthService,
		private router: Router
	) {}

	passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
		const password = group.get('password')?.value;
		const confirm = group.get('confirmPassword')?.value;
		return password && confirm && password !== confirm ? { passwordsMismatch: true } : null;
	}

	submit() {
		if (this.form.invalid) {
			this.form.markAllAsTouched();
			return;
		}

		this.isSubmitting = true;
		const { firstName, lastName, email, password, confirmPassword } = this.form.value as { firstName: string; lastName: string; email: string; password: string; confirmPassword: string };

		this.auth.register({ firstName, lastName, email, password, confirmPassword }).subscribe({
			next: (response) => {
				this.isSubmitting = false;
				// If backend returns token/user (auto login), go home; otherwise instruct to verify email then go to login
				if (response && response.token) {
					this.router.navigateByUrl('/');
				} else {
					alert('Registration successful. Please check your email to confirm your account.');
					this.router.navigateByUrl('/login');
				}
			},
			error: (err) => {
				this.isSubmitting = false;
				const message = (err?.error && (err.error.message || err.error.title)) || err?.message || 'Registration failed. Please try again.';
				alert(message);
			}
		});
	}
}


