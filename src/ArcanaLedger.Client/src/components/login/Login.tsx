import { useState, type FormEvent } from 'react';
import type { AuthenticatedUser, AuthProvider } from '../../auth/authentication';

interface LoginProps {
  authProvider: AuthProvider;
  onAuthenticated: (user: AuthenticatedUser) => void;
  onNavigate: (page: string) => void;
}

export function Login({ authProvider, onAuthenticated, onNavigate }: LoginProps) {
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const user = await authProvider.signIn({
      email: String(formData.get('email') ?? ''),
      password: String(formData.get('password') ?? ''),
    });

    if (!user) {
      setError('The email or password is incorrect.');
      return;
    }

    setError('');
    setSubmitted(true);
    onAuthenticated(user);
  };

  return (
    <main className="login-page">
      <section className="login-shell" aria-label="Trading portfolio sign in">
        <section className="login-brand-panel">
          <div className="login-brand-content">
            <span className="login-logo" role="img" aria-label="Trading logo" />
            <p className="login-eyebrow">PORTFOLIO OPERATIONS</p>
            <h1>Trade with a clearer view.</h1>
            <p className="login-brand-copy">Monitor positions, model fair value, and keep portfolio risk close at hand.</p>
          </div>
          <div className="login-brand-footer"><span aria-hidden="true" />Markets and portfolio tools are ready</div>
        </section>

        <section className="login-form-panel">
          <h2>Welcome back</h2>
          <p className="login-form-intro">Sign in to continue to your trading workspace.</p>
          <form className="login-form" onSubmit={handleSubmit}>
            <div className="login-field">
              <label htmlFor="email">Email address</label>
              <input id="email" name="email" type="email" autoComplete="email" placeholder="you@example.com" required />
            </div>
            <div className="login-field">
              <div className="login-password-row">
                <label htmlFor="password">Password</label>
                <a href="#forgot-password">Forgot password?</a>
              </div>
              <input id="password" name="password" type="password" autoComplete="current-password" placeholder="Enter your password" required />
            </div>
            <div className="login-options">
              <label className="login-remember"><input type="checkbox" name="remember" /> Remember me</label>
              <span>Secure sign in</span>
            </div>
            {error && <p className="login-error" role="alert">{error}</p>}
            <button className="login-submit" type="submit">{submitted ? 'Signed in' : 'Sign in'}</button>
          </form>
          <div className="login-divider"><span>New to the workspace?</span></div>
          <p className="login-signup"><a href="#create-account" onClick={(event) => { event.preventDefault(); onNavigate('Create account'); }}>Create an account</a></p>
        </section>
      </section>
    </main>
  );
}