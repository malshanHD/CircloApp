import { BrowserRouter } from "react-router-dom";
import { MotionConfig } from "framer-motion";
import AppRoutes from "./routes/AppRoutes";
import AuthProvider from "./context/AuthContext";
export default function App() {
  return (
    <BrowserRouter>
      <MotionConfig reducedMotion="user">
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </MotionConfig>
    </BrowserRouter>
  );
}
