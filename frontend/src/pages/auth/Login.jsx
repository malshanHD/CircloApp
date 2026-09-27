import { useContext, useEffect, useRef, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { FiArrowRight, FiCheck } from "react-icons/fi";
import { GoogleLogin, useGoogleOAuth } from "@react-oauth/google";
import { authService } from "../../services/authService";
import { useAuth } from "../../hooks/useAuth";
import AuthLayout from "../../layouts/AuthLayout";
import { Field, ApiError, Success } from "../../components/common/UI";
import { GoogleScriptErrorContext } from "../../context/GoogleScriptErrorContext";

import { loginDestination } from "../../utils/loginDestination";

export default function Login() {
  const { session, login } = useAuth();
  const { scriptLoadedSuccessfully } = useGoogleOAuth();
  const scriptError = useContext(GoogleScriptErrorContext);
  const location = useLocation();
  const navigate = useNavigate();
  const destination = loginDestination(location.state?.from);
  const { register, handleSubmit, formState: { errors } } = useForm();
  const inFlight = useRef(false);
  const [googleError, setGoogleError] = useState(null);
  const [welcome, setWelcome] = useState(null);
  const mutation = useMutation({
    mutationFn: async ({ path, data }) => {
      const response = await authService(path, data);
      if (!response.data?.accessToken) throw new Error("Sign-in could not be completed. Please try again.");
      return response.data;
    },
    onSuccess: setWelcome,
    onError: () => { inFlight.current = false; },
  });
  const busy = mutation.isPending || Boolean(welcome);
  useEffect(() => {
    if (!welcome) return;
    const delay = window.matchMedia("(prefers-reduced-motion: reduce)").matches ? 0 : 450;
    const timer = setTimeout(() => { login(welcome); navigate(destination, { replace: true }); }, delay);
    return () => clearTimeout(timer);
  }, [welcome, login, navigate, destination]);

  function submit(path, data) {
    if (inFlight.current) return;
    inFlight.current = true;
    setGoogleError(null);
    mutation.mutate({ path, data });
  }
  function handleGoogleSuccess(response) {
    if (inFlight.current) return;
    if (!response.credential?.trim()) {
      setGoogleError(new Error("Google did not return a sign-in credential. Please try again."));
      return;
    }
    submit("/auth/google", { idToken: response.credential });
  }
  if (session && !welcome) return <Navigate to={destination} replace />;
  return <AuthLayout title="Welcome back." subtitle="Your next great plan starts here." className="login-layout">
    {location.state?.verified && <Success>Email verified. You're ready to sign in.</Success>}
    <div className={`login-interaction${welcome ? " login-complete" : ""}`} aria-busy={busy}>
      {welcome && <div className="login-success" role="status"><span className="login-check"><FiCheck aria-hidden="true" /></span><strong>You're in.</strong><span>Taking you to your circle…</span></div>}
      <form noValidate onSubmit={event => handleSubmit(data => submit("/auth/login", data))(event)} className="form-stack">
        <fieldset disabled={busy} className="form-stack login-fields">
          <Field label="Email or username" autoComplete="username" placeholder="you@example.com" registration={register("usernameOrEmail", { required: "Enter your email or username." })} error={errors.usernameOrEmail} />
          <Field label="Password" type="password" autoComplete="current-password" placeholder="Your password" registration={register("password", { required: "Enter your password." })} error={errors.password} />
          <ApiError error={googleError || mutation.error} />
          <button className="button primary wide" disabled={busy}>
            {busy ? <><span className="login-spinner" aria-hidden="true" />{welcome ? "Signed in" : "Signing in…"}</> : <>Sign in <FiArrowRight /></>}
          </button>
        </fieldset>
      </form>
      <div className="auth-divider"><span>or continue with</span></div>
      <div className="google-login-container" inert={busy || !scriptLoadedSuccessfully || Boolean(scriptError)} aria-disabled={busy || !scriptLoadedSuccessfully || Boolean(scriptError)}>
        <GoogleLogin onSuccess={handleGoogleSuccess}
          onError={() => { if (!inFlight.current) setGoogleError(new Error("Google sign-in was not completed. Please try again or use your password.")); }}
          text="continue_with" shape="pill" size="large" theme="outline" width="260" />
      </div>
      <ApiError error={scriptError} />
      {!scriptLoadedSuccessfully && !scriptError && <p className="google-status" role="status">Loading Google sign-in…</p>}
      {mutation.isPending && <p className="google-status" role="status">Securely signing you in…</p>}
      <p className="form-switch">New to Circlo? <Link to="/register">Create an account</Link></p>
    </div>
  </AuthLayout>;
}
