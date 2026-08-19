import { Routes, Route } from "react-router-dom";

import Login from "../pages/auth/Login";
import Register from "../pages/auth/Register";
import Dashboard from "../pages/dashboard/Dashboard";
import OtpVerification from "../pages/auth/OtpVerification";
import ProtectedRoute from "../components/ProtectedRoute";

function AppRoutes() {
    return (
        <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            {/* <Route path="/dashboard" element={<Dashboard />} /> */}
            <Route path="/verify-otp" element={<OtpVerification />} />

            <Route element={<ProtectedRoute />}>
                <Route path = "/dashboard" element={<Dashboard />} />
            </Route>
        </Routes>
    );
}

export default AppRoutes;