import { useEffect, useState } from "react";
const messages = [
  "Waking up Circlo… 😴",
  "Our server was having a tiny nap ☕",
  "Stretching the API… almost ready 🧘",
  "Connecting the circles… ✨",
  "Almost there!",
];
export default function LoginWaiting({ signingIn }) {
  const [index, setIndex] = useState(0);
  useEffect(() => {
    const timer = setInterval(() => setIndex(value => (value + 1) % messages.length), 4500);
    return () => clearInterval(timer);
  }, []);
  return <div className="cold-start-screen" role="status" aria-live="polite" aria-atomic="true">
    <div className="cold-start-orbit" aria-hidden="true">
      <span className="brand-mark"><i /><i /><i /></span>
      <span className="cold-start-satellite" />
    </div>
    <span className="eyebrow">A LITTLE MORE TOGETHER</span>
    <h2 key={signingIn ? "ready" : index}>{signingIn ? "Circlo is awake. Signing you in…" : messages[index]}</h2>
    <p>{signingIn ? "Getting your circle ready." : "A quiet moment before the good times. We’ll continue your sign-in automatically."}</p>
    <div className="cold-start-dots" aria-hidden="true"><i /><i /><i /></div>
    <small>You can leave this page to cancel.</small>
  </div>;
}
