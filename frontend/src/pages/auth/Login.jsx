import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { FiArrowRight } from "react-icons/fi";
import { authService } from "../../services/authService";
import { useAuth } from "../../hooks/useAuth";
import AuthLayout from "../../layouts/AuthLayout";
import { Field, ApiError, Success } from "../../components/common/UI";
export default function Login() {
  const { session, login } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();
  const requested = location.state?.from;
  const destination =
    typeof requested === "string" &&
    requested.startsWith("/") &&
    !requested.startsWith("//")
      ? requested
      : "/dashboard";
  const mutation = useMutation({
    mutationFn: (data) => authService("/auth/login", data),
    onSuccess: (response) => {
      if (!response.data?.accessToken)
        throw new Error("The API did not return an access token.");
      login(response.data);
      navigate(destination, { replace: true });
    },
  });
  if (session) return <Navigate to={destination} replace />;
  return (
    <AuthLayout
      title="Welcome back."
      subtitle="Your next great plan starts here."
    >
      {location.state?.verified && (
        <Success>Email verified. You're ready to sign in.</Success>
      )}
      <form
        noValidate
        onSubmit={handleSubmit((data) => {
          if (!mutation.isPending) mutation.mutate(data);
        })}
        className="form-stack"
      >
        <Field
          label="Email or username"
          autoComplete="username"
          placeholder="you@example.com"
          registration={register("usernameOrEmail", {
            required: "Enter your email or username.",
          })}
          error={errors.usernameOrEmail}
        />
        <Field
          label="Password"
          type="password"
          autoComplete="current-password"
          placeholder="Your password"
          registration={register("password", {
            required: "Enter your password.",
          })}
          error={errors.password}
        />
        <ApiError error={mutation.error} />
        <button className="button primary wide" disabled={mutation.isPending}>
          {mutation.isPending ? (
            "Signing in…"
          ) : (
            <>
              Sign in <FiArrowRight />
            </>
          )}
        </button>
      </form>
      <p className="form-switch">
        New to Circlo? <Link to="/register">Create an account</Link>
      </p>
    </AuthLayout>
  );
}
