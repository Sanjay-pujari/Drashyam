import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

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

	constructor(private fb: FormBuilder) {}

	submit() {
		if (this.form.invalid) return;
		console.log('Login submit', this.form.value);
	}
}



