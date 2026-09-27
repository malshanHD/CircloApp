import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import GoogleAuthProvider from "./components/GoogleAuthProvider";
import "./index.css";
import App from "./App.jsx";

const queryClient = new QueryClient();
const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;

createRoot(document.getElementById("root")).render(
  <GoogleAuthProvider clientId={googleClientId}>
    <QueryClientProvider client={queryClient}><App /></QueryClientProvider>
  </GoogleAuthProvider>,
);
