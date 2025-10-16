import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
	selector: 'app-login',
	standalone: true,
	imports: [CommonModule, ReactiveFormsModule],
	templateUrl: './login.component.html',
	styleUrls: ['./login.component.scss']
})
export class LoginComponent {
	form = this.fb.group({
		email: ['', [Validators.required, Validators.email]],
		password: ['', [Validators.required]]
	});

	constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {}

	submit() {
		if (this.form.invalid) return;
		const { email, password } = this.form.value as { email: string; password: string };
		this.auth.login({ email, password, rememberMe: false }).subscribe({
			next: () => this.router.navigateByUrl('/'),
			error: (err) => console.error('Login failed', err)
		});
	}
}



