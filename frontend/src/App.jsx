import { BrowserRouter } from "react-router-dom";
import { MotionConfig } from "framer-motion";
import AppRoutes from "./routes/AppRoutes";
import AuthProvider from "./context/AuthContext";
import ThemeProvider from "./context/ThemeContext";
export default function App() {
  return (
    <ThemeProvider><BrowserRouter>
      <MotionConfig reducedMotion="user">
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </MotionConfig>
    </BrowserRouter></ThemeProvider>
  );
}
