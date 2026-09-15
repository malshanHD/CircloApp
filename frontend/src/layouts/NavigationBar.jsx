import { NavLink } from "react-router-dom";
import { FiGrid, FiLayers, FiLogOut, FiZap, FiPlus } from "react-icons/fi";
import { Brand } from "../components/common/UI";
import { useAuth } from "../hooks/useAuth";
export default function Navigation({ close = () => {}, create }) {
  const { session, logout } = useAuth();
  return (
    <div className="sidebar-content">
      <Brand />
      <p className="nav-caption">YOUR SPACE</p>
      <nav aria-label="Main navigation">
        <NavLink to="/dashboard" onClick={close}>
          <FiGrid /> Overview
        </NavLink>
        <NavLink to="/events" onClick={close}>
          <FiLayers /> My events
        </NavLink>
        <NavLink to="/assistant" onClick={close}>
          <FiZap /> Circlo AI <span className="tiny-badge">ASK</span>
        </NavLink>
      </nav>
      <button
        className="button primary wide sidebar-create"
        onClick={() => {
          close();
          create();
        }}
      >
        <FiPlus /> Create event
      </button>
      <div className="sidebar-bottom">
        <div className="sidebar-note">
          <span className="note-symbol">✳</span>
          <h3>Better, together.</h3>
          <p>
            Make room for memories.
            <br />
            We'll keep things organized.
          </p>
        </div>
        <div className="profile-area">
          <span className="avatar">
            {session?.username?.slice(0, 1).toUpperCase()}
          </span>
          <div>
            <strong>{session?.username}</strong>
            <span>Your personal space</span>
          </div>
          <button className="icon-button" onClick={logout} aria-label="Log out">
            <FiLogOut />
          </button>
        </div>
      </div>
    </div>
  );
}
