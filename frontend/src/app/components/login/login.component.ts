import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/conversation.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  imports: [
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
})
export class LoginComponent {
  emailValue = '';
  loading = signal(false);

  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  onLogin(): void {
    if (!this.emailValue || !this.emailValue.includes('@')) {
      this.snackBar.open('Please enter a valid email address', 'Close', {
        duration: 3000,
        horizontalPosition: 'center',
        verticalPosition: 'top',
      });
      return;
    }

    this.loading.set(true);

    const loginRequest: LoginRequest = {
      email: this.emailValue,
    };

    this.authService.login(loginRequest).subscribe({
      next: () => {
        this.loading.set(false);
        this.snackBar.open('Login successful!', 'Close', {
          duration: 2000,
          horizontalPosition: 'center',
          verticalPosition: 'top',
        });
        this.router.navigate(['/chat']);
      },
      error: (error) => {
        this.loading.set(false);
        this.snackBar.open('Login failed. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'center',
          verticalPosition: 'top',
        });
        console.error('Login error:', error);
      },
    });
  }
}
