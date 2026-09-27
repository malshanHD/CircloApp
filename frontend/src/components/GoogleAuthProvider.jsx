import { useState } from "react";
import { GoogleOAuthProvider } from "@react-oauth/google";
import { GoogleScriptErrorContext } from "../context/GoogleScriptErrorContext";

export default function GoogleAuthProvider({ clientId, children }) {
  const [scriptError, setScriptError] = useState(null);
  if (!clientId?.trim() || clientId.includes("YOUR_GOOGLE")) {
    return <main className="startup-error" role="alert">
      <h1>Circlo sign-in needs configuration.</h1>
      <p>VITE_GOOGLE_CLIENT_ID is missing. Configure the public Google Web Client ID and restart or rebuild the frontend.</p>
    </main>;
  }
  return <GoogleOAuthProvider clientId={clientId.trim()}
    onScriptLoadError={() => setScriptError(new Error("Google sign-in could not load. Check your connection or sign in with your password."))}>
    <GoogleScriptErrorContext.Provider value={scriptError}>{children}</GoogleScriptErrorContext.Provider>
  </GoogleOAuthProvider>;
}
