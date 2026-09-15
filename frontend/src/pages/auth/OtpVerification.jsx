import { useMutation } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { authService } from "../../services/authService";
import AuthLayout from "../../layouts/AuthLayout";
import { ApiError, Success } from "../../components/common/UI";
function getPending() {
  try {
    return JSON.parse(sessionStorage.getItem("circlo-registration")) || {};
  } catch {
    return {};
  }
}
export default function OtpVerification() {
  const location = useLocation();
  const navigate = useNavigate();
  const [pending] = useState(() =>
    location.state?.email ? location.state : getPending(),
  );
  const [otp, setOtp] = useState(Array(6).fill(""));
  const [now, setNow] = useState(Date.now);
  const [invalid, setInvalid] = useState(false);
  const refs = useRef([]);
  const remaining = Math.max(0, Math.ceil((pending.expiresAt - now) / 1000));
  useEffect(() => {
    const timer = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(timer);
  }, []);
  const mutation = useMutation({
    mutationFn: (data) => authService("/auth/verify-email", data),
    onSuccess: () => {
      sessionStorage.removeItem("circlo-registration");
    },
  });
  useEffect(() => {
    if (mutation.isSuccess) {
      const timer = setTimeout(
        () => navigate("/login", { replace: true, state: { verified: true } }),
        1200,
      );
      return () => clearTimeout(timer);
    }
  }, [mutation.isSuccess, navigate]);
  if (!pending.email) return <Navigate to="/register" replace />;
  function enter(value, index) {
    const digits = value.replace(/\D/g, "").slice(0, 6 - index);
    setOtp((old) => {
      const next = [...old];
      next[index] = "";
      [...digits].forEach((digit, offset) => {
        next[index + offset] = digit;
      });
      return next;
    });
    if (digits) refs.current[Math.min(index + digits.length, 5)]?.focus();
  }
  return (
    <AuthLayout
      title="Check your inbox."
      subtitle={`We've sent a six-digit code to ${pending.email}.`}
    >
      {mutation.isSuccess ? (
        <Success>Email verified. Taking you to sign in…</Success>
      ) : (
        <form
          className="form-stack"
          onSubmit={(e) => {
            e.preventDefault();
            if (otp.join("").length !== 6) {
              setInvalid(true);
              refs.current[otp.findIndex((x) => !x)]?.focus();
              return;
            }
            if (!mutation.isPending)
              mutation.mutate({ email: pending.email, otp: otp.join("") });
          }}
        >
          <fieldset disabled={mutation.isPending}>
            <legend>Verification code</legend>
            <div className="otp-inputs">
              {otp.map((digit, index) => (
                <input
                  key={index}
                  ref={(el) => {
                    refs.current[index] = el;
                  }}
                  aria-label={`Digit ${index + 1}`}
                  aria-invalid={invalid}
                  inputMode="numeric"
                  autoComplete={index === 0 ? "one-time-code" : "off"}
                  value={digit}
                  onChange={(e) => enter(e.target.value, index)}
                  onPaste={(e) => {
                    e.preventDefault();
                    enter(e.clipboardData.getData("text"), index);
                  }}
                  onFocus={(e) => e.target.select()}
                  onKeyDown={(e) => {
                    if (e.key === "Backspace" && !digit && index > 0)
                      refs.current[index - 1]?.focus();
                    if (e.key === "ArrowLeft") refs.current[index - 1]?.focus();
                    if (e.key === "ArrowRight")
                      refs.current[index + 1]?.focus();
                  }}
                />
              ))}
            </div>
          </fieldset>
          {invalid && otp.join("").length !== 6 && (
            <p className="field-error">Enter all six digits.</p>
          )}
          <ApiError error={mutation.error} />
          <button className="button primary wide" disabled={mutation.isPending}>
            {mutation.isPending ? "Verifying…" : "Verify email"}
          </button>
          <p className="muted small">
            {remaining > 0
              ? `Code window: ${Math.floor(remaining / 60)}:${String(remaining % 60).padStart(2, "0")}`
              : "The expected code window has elapsed. If verification fails, register again."}
          </p>
          <p className="muted small">
            Didn't receive a code? Check your spam folder. Resending isn't
            available yet.
          </p>
          <Link to="/register" className="text-button">
            Return to registration
          </Link>
        </form>
      )}
    </AuthLayout>
  );
}
