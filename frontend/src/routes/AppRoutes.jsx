import { Routes, Route, Navigate, Link } from "react-router-dom";
import Login from "../pages/auth/Login";
import Register from "../pages/auth/Register";
import { lazy, Suspense } from "react";
import { Skeleton } from "../components/common/UI";
const Dashboard = lazy(() => import("../pages/dashboard/Dashboard"));
import OtpVerification from "../pages/auth/OtpVerification";
import ProtectedRoute from "../components/ProtectedRoute";
import Events from "../pages/events/Events";
const EventDetails = lazy(() => import("../pages/events/EventDetails"));
import AcceptInvitation from "../pages/events/AcceptInvitation";
import MainLayout from "../layouts/MainLayout";
export default function AppRoutes() {
  return (
    <Suspense
      fallback={
        <div className="main-content">
          <Skeleton />
        </div>
      }
    >
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/verify-otp" element={<OtpVerification />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<MainLayout />}>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/events" element={<Events />} />
            <Route path="/events/:eventId" element={<EventDetails />} />
            <Route path="/assistant" element={<Events assistant />} />
            <Route path="/accept-invite" element={<AcceptInvitation />} />
          </Route>
        </Route>
        <Route
          path="*"
          element={
            <div className="empty">
              <h1>This page wandered off.</h1>
              <p>Let's get you back to your circle.</p>
              <Link className="button primary" to="/dashboard">
                Back to dashboard
              </Link>
            </div>
          }
        />
      </Routes>
    </Suspense>
  );
}
