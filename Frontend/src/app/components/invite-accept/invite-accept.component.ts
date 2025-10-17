import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { InviteService } from '../../services/invite.service';
import { AuthService } from '../../services/auth.service';
import { UserInvite, AcceptInvite } from '../../models/invite.model';

@Component({
  selector: 'app-invite-accept',
  templateUrl: './invite-accept.component.html',
  styleUrls: ['./invite-accept.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe]
})
export class InviteAcceptComponent implements OnInit {
  inviteForm: FormGroup;
  invite: UserInvite | null = null;
  loading = false;
  error: string | null = null;
  token: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private inviteService: InviteService,
    private authService: AuthService
  ) {
    this.inviteForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.token = params['token'];
      if (this.token) {
        this.loadInvite();
      }
    });
  }

  loadInvite(): void {
    if (!this.token) return;

    this.loading = true;
    this.inviteService.getInviteByToken(this.token).subscribe({
      next: (invite) => {
        this.invite = invite;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Invalid or expired invitation link.';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.inviteForm.valid && this.token) {
      this.loading = true;
      const acceptData: AcceptInvite = {
        inviteToken: this.token,
        firstName: this.inviteForm.value.firstName,
        lastName: this.inviteForm.value.lastName,
        password: this.inviteForm.value.password
      };

      this.inviteService.acceptInvite(this.token, acceptData).subscribe({
        next: (invite) => {
          // Auto-login the user after successful registration
          this.authService.login({
            email: invite.inviteeEmail,
            password: acceptData.password,
            rememberMe: false
          }).subscribe({
            next: () => {
              this.router.navigate(['/']);
            },
            error: (loginError) => {
              console.error('Auto-login failed:', loginError);
              this.router.navigate(['/login']);
            }
          });
        },
        error: (error) => {
          this.error = error.error?.message || 'Failed to accept invitation. Please try again.';
          this.loading = false;
        }
      });
    }
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  getFieldError(fieldName: string): string {
    const field = this.inviteForm.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) {
        return `${fieldName} is required`;
      }
      if (field.errors['minlength']) {
        return `${fieldName} must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
      if (field.errors['passwordMismatch']) {
        return 'Passwords do not match';
      }
    }
    return '';
  }
}