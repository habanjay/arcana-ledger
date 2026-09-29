import { useState, type FormEvent } from 'react';

interface CreateAccountProps {
  onNavigate: (page: string) => void;
}

export function CreateAccount({ onNavigate }: CreateAccountProps) {
  const [submitted, setSubmitted] = useState(false);
  const [passwordError, setPasswordError] = useState('');

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    const password = form.elements.namedItem('password');
    const confirmation = form.elements.namedItem('confirm-password');

    if (!(password instanceof HTMLInputElement) || !(confirmation instanceof HTMLInputElement)) return;

    if (password.value !== confirmation.value) {
      setPasswordError('Passwords must match.');
      confirmation.focus();
      return;
    }

    setPasswordError('');
    setSubmitted(true);
  };

  return (
    <main className="create-account-page">
      <section className="create-account-shell" aria-label="Create a trading portfolio account">
        <section className="create-account-brand-panel">
          <div className="create-account-brand-content">
            <span className="create-account-logo" role="img" aria-label="Trading logo" />
            <p className="create-account-eyebrow">PORTFOLIO OPERATIONS</p>
            <h1>Build a clearer view of every trade.</h1>
            <p className="create-account-brand-copy">Create your workspace to monitor positions, model fair value, and keep portfolio risk close at hand.</p>
          </div>
          <div className="create-account-brand-footer"><span aria-hidden="true" />Private, focused portfolio tools</div>
        </section>

        <section className="create-account-form-panel">
          <h2>Create your account</h2>
          <p className="create-account-form-intro">Set up your trading workspace in a few quick steps.</p>
          <form className="create-account-form" onSubmit={handleSubmit} noValidate={false}>
            <div className="create-account-field">
              <label htmlFor="create-account-name">Full name</label>
              <input id="create-account-name" name="name" type="text" autoComplete="name" placeholder="Alex Morgan" required />
            </div>
            <div className="create-account-field">
              <label htmlFor="create-account-email">Email address</label>
              <input id="create-account-email" name="email" type="email" autoComplete="email" placeholder="you@example.com" required />
            </div>
            <div className="create-account-field">
              <label htmlFor="create-account-password">Password</label>
              <input id="create-account-password" name="password" type="password" autoComplete="new-password" placeholder="At least 8 characters" minLength={8} required />
            </div>
            <div className="create-account-field">
              <label htmlFor="create-account-confirm-password">Confirm password</label>
              <input id="create-account-confirm-password" name="confirm-password" type="password" autoComplete="new-password" placeholder="Re-enter your password" minLength={8} required aria-describedby={passwordError ? 'password-error' : undefined} />
              {passwordError && <span className="create-account-error" id="password-error" role="alert">{passwordError}</span>}
            </div>
            <label className="create-account-terms"><input name="terms" type="checkbox" required /><span>I agree to the <a href="#terms">Terms of service</a> and <a href="#privacy">Privacy policy</a>.</span></label>
            <button className="create-account-submit" type="submit" disabled={submitted}>{submitted ? 'Account created' : 'Create account'}</button>
          </form>
          <div className="create-account-divider"><span>Already have an account?</span></div>
          <p className="create-account-signin"><a href="#login" onClick={(event) => { event.preventDefault(); onNavigate('Login'); }}>Sign in to your workspace</a></p>
        </section>
      </section>
    </main>
  );
}