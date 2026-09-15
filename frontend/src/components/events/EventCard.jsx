import { Link } from "react-router-dom";
import { motion, useReducedMotion } from "framer-motion";
import {
  FiArrowUpRight,
  FiUsers,
  FiCalendar,
  FiArrowRight,
} from "react-icons/fi";
import { formatDate } from "../../utils/format";
export default function EventCard({ event, index = 0 }) {
  const reduced = useReducedMotion();
  return (
    <motion.article
      initial={{ opacity: 0, y: reduced ? 0 : 12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{
        duration: 0.24,
        delay: reduced ? 0 : Math.min(index * 0.04, 0.2),
      }}
      className={`event-card card tone-${index % 3}`}
    >
      <div className="event-art" aria-hidden="true">
        <span className="event-art-ring" />
        <span className="event-art-icon">
          <FiArrowUpRight />
        </span>
        <span className="event-art-dot" />
      </div>
      <div className="event-card-body">
        <div className="event-card-heading">
          <span className="badge">
            {event.isAdmin ? "Created by you" : "Shared event"}
          </span>
          <span className="muted small">
            <FiUsers /> {event.memberCount}
          </span>
        </div>
        <h3>
          <Link to={`/events/${event.id}`}>{event.name}</Link>
        </h3>
        <p className="event-description">
          {event.description || "A shared space for your group's plans."}
        </p>
        <div className="event-meta">
          <span>
            <FiCalendar /> {formatDate(event.createdAt)}
          </span>
          <Link to={`/events/${event.id}`} aria-label={`View ${event.name}`}>
            <FiArrowRight />
          </Link>
        </div>
      </div>
    </motion.article>
  );
}
