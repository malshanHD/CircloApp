import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Link, Navigate, useNavigate } from "react-router-dom";
import { authService } from "../../services/authService";
import { useAuth } from "../../hooks/useAuth";
import AuthLayout from "../../layouts/AuthLayout";
import { Field, ApiError } from "../../components/common/UI";
export default function Register() {
  const { session } = useAuth();
  const navigate = useNavigate();
  const {
    register,
    getValues,
    handleSubmit,
    formState: { errors },
  } = useForm();
  const mutation = useMutation({
    mutationFn: (data) => authService("/auth/register", data),
    onSuccess: (response) => {
      if (response.data?.requiresOtpVerification) {
        const pending = {
          email: response.data.email,
          expiresAt: Date.now() + 5 * 60 * 1000,
        };
        sessionStorage.setItem("circlo-registration", JSON.stringify(pending));
        navigate("/verify-otp", { state: pending });
      } else navigate("/login");
    },
  });
  const nameRules = (label) => ({
    required: `Enter your ${label}.`,
    maxLength: { value: 50, message: "Use 50 characters or fewer." },
    validate: (value) => Boolean(value.trim()) || "This field cannot be blank.",
  });
  if (session) return <Navigate to="/dashboard" replace />;
  return (
    <AuthLayout
      title="Find your circle."
      subtitle="Create an account and bring your next plan to life."
    >
      <form
        noValidate
        className="form-stack"
        onSubmit={handleSubmit(({ confirmPassword, ...data }) => {
          void confirmPassword;
          if (!mutation.isPending) mutation.mutate(data);
        })}
      >
        <div className="form-grid">
          <Field
            label="First name"
            autoComplete="given-name"
            registration={register("firstName", nameRules("first name"))}
            error={errors.firstName}
          />
          <Field
            label="Last name"
            autoComplete="family-name"
            registration={register("lastName", nameRules("last name"))}
            error={errors.lastName}
          />
        </div>
        <Field
          label="Email address"
          type="email"
          autoComplete="email"
          placeholder="you@example.com"
          registration={register("email", {
            required: "Enter your email.",
            pattern: {
              value: /^[^@\s]+@[^@\s]+$/,
              message: "Enter a valid email address.",
            },
          })}
          error={errors.email}
        />
        <div className="form-grid">
          <Field
            label="Username"
            autoComplete="username"
            registration={register("username", nameRules("username"))}
            error={errors.username}
          />
          <Field
            label="Contact number (optional)"
            type="tel"
            autoComplete="tel"
            registration={register("contactNumber")}
          />
        </div>
        <div className="form-grid">
          <Field
            label="Password"
            type="password"
            autoComplete="new-password"
            registration={register("password", {
              required: "Enter a password.",
              minLength: { value: 8, message: "Use at least 8 characters." },
            })}
            error={errors.password}
          />
          <Field
            label="Confirm password"
            type="password"
            autoComplete="new-password"
            registration={register("confirmPassword", {
              required: "Confirm your password.",
              validate: (value) =>
                value === getValues("password") || "Passwords do not match.",
            })}
            error={errors.confirmPassword}
          />
        </div>
        <ApiError error={mutation.error} />
        <button className="button primary wide" disabled={mutation.isPending}>
          {mutation.isPending ? "Creating your account…" : "Create account"}
        </button>
      </form>
      <p className="form-switch">
        Already part of the circle? <Link to="/login">Sign in</Link>
      </p>
    </AuthLayout>
  );
}
