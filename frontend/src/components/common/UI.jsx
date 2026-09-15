import { useId, useRef, useEffect, useState } from "react";
import { motion, useReducedMotion } from "framer-motion";
import {
  FiArrowUpRight,
  FiEye,
  FiEyeOff,
  FiX,
  FiAlertCircle,
  FiCheckCircle,
} from "react-icons/fi";
import { Link } from "react-router-dom";
import { getApiError } from "../../utils/apiError";
export function Brand() {
  return (
    <Link to="/dashboard" className="brand" aria-label="Circlo home">
      <span className="brand-mark" aria-hidden="true">
        <i />
        <i />
        <i />
      </span>
      circlo<span className="brand-dot">.</span>
    </Link>
  );
}
export function Page({ children, className = "" }) {
  const reduced = useReducedMotion();
  return (
    <motion.div
      className={className}
      initial={{ opacity: 0, y: reduced ? 0 : 10 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.22 }}
    >
      {children}
    </motion.div>
  );
}
export function Field({ label, error, type = "text", registration, ...props }) {
  const id = useId();
  const [visible, setVisible] = useState(false);
  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <div className="input-wrap">
        <input
          id={id}
          type={type === "password" && visible ? "text" : type}
          {...registration}
          {...props}
          aria-invalid={Boolean(error)}
          aria-describedby={error ? id + "-error" : undefined}
        />
        {type === "password" && (
          <button
            type="button"
            className="password-toggle"
            onClick={() => setVisible(!visible)}
            aria-label={visible ? "Hide password" : "Show password"}
          >
            {visible ? <FiEyeOff /> : <FiEye />}
          </button>
        )}
      </div>
      {error && (
        <p id={id + "-error"} className="field-error">
          {error.message}
        </p>
      )}
    </div>
  );
}
export function ApiError({ error, retry }) {
  if (!error) return null;
  const { message, errors } = getApiError(error);
  return (
    <div className="error-box" role="alert">
      <FiAlertCircle aria-hidden="true" />
      <div>
        <strong>{message}</strong>
        {errors.length > 0 && (
          <ul>
            {errors.map((item, i) => (
              <li key={i}>{item}</li>
            ))}
          </ul>
        )}
        {retry && (
          <button className="text-button" onClick={retry}>
            Try again
          </button>
        )}
      </div>
    </div>
  );
}
export function Success({ children }) {
  return (
    <div role="status" className="success-box">
      <FiCheckCircle />
      {children}
    </div>
  );
}
export function Skeleton({ count = 3 }) {
  return (
    <div className="event-grid" role="status" aria-label="Loading">
      <span className="sr-only">Loading…</span>
      {Array.from({ length: count }, (_, i) => (
        <div key={i} className="skeleton card">
          <div />
          <div />
          <div />
        </div>
      ))}
    </div>
  );
}
export function Empty({ title, children, action }) {
  return (
    <Page className="empty card">
      <span className="empty-icon">
        <FiArrowUpRight />
      </span>
      <h3>{title}</h3>
      <p>{children}</p>
      {action}
    </Page>
  );
}
export function Modal({
  title,
  children,
  onClose,
  busy = false,
  className = "",
}) {
  const ref = useRef(null);
  const id = useId();
  useEffect(() => {
    const dialog = ref.current;
    const previous = document.activeElement;
    dialog.showModal();
    const overflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      dialog.close();
      document.body.style.overflow = overflow;
      previous?.focus();
    };
  }, []);
  return (
    <dialog
      ref={ref}
      className={`modal ${className}`}
      aria-labelledby={id}
      onCancel={(e) => {
        e.preventDefault();
        if (!busy) onClose();
      }}
      onClick={(e) => {
        if (e.target === e.currentTarget && !busy) onClose();
      }}
    >
      <div className="modal-inner">
        <div className="section-heading">
          <h2 id={id}>{title}</h2>
          <button
            className="icon-button"
            aria-label="Close dialog"
            onClick={onClose}
            disabled={busy}
          >
            <FiX />
          </button>
        </div>
        {children}
      </div>
    </dialog>
  );
}
