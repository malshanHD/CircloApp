import { useState } from "react";
import { Modal } from "../../components/common/UI";
export default function AddMemberModal({
  isOpen,
  onClose,
  eventName,
  eventId,
}) {
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState(false);
  const link = `${window.location.origin}/accept-invite?eventId=${encodeURIComponent(eventId)}`;
  const text = `Join ${eventName} on Circlo: ${link}`;
  if (!isOpen) return null;
  async function copy() {
    setBusy(true);
    try {
      await navigator.clipboard.writeText(link);
      setMessage("Link copied. Paste it into WhatsApp or any chat.");
    } catch {
      setMessage("Copy the link from the field above.");
    } finally {
      setBusy(false);
    }
  }
  async function share() {
    setBusy(true);
    try {
      await navigator.share({ title: eventName, text, url: link });
    } catch (error) {
      if (error.name !== "AbortError")
        setMessage("Sharing is unavailable. Copy the link instead.");
    } finally {
      setBusy(false);
    }
  }
  return (
    <Modal title="Share your event." onClose={onClose} busy={busy}>
      <p>Send this link to the people joining {eventName}.</p>
      <p className="muted small">
        They must sign in to an existing Circlo account and request to join. You
        approve each request before they can access the event.
      </p>
      <div className="field">
        <label htmlFor="event-invite-link">Event link</label>
        <input
          id="event-invite-link"
          readOnly
          value={link}
          onFocus={(e) => e.target.select()}
        />
      </div>
      <p role="status" className="small">
        {message}
      </p>
      <div className="form-actions">
        <button className="button primary" disabled={busy} onClick={copy}>
          Copy link
        </button>
        <a
          className="button secondary"
          href={`https://wa.me/?text=${encodeURIComponent(text)}`}
          target="_blank"
          rel="noreferrer"
        >
          WhatsApp
        </a>
        {typeof navigator.share === "function" && (
          <button className="button secondary" disabled={busy} onClick={share}>
            Share…
          </button>
        )}
      </div>
    </Modal>
  );
}
