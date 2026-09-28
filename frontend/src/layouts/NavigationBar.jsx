import { NavLink } from "react-router-dom";
import { FiLogOut, FiPlus } from "react-icons/fi";
import { Brand } from "../components/common/UI";
import { useAuth } from "../hooks/useAuth";
import { useWorkspace } from "../features/workspace/WorkspaceContext";
import WorkspaceSwitcher from "../features/workspace/WorkspaceSwitcher";

export default function Navigation({ close = () => {}, primaryAction }) {
  const { session, logout } = useAuth();
  const { mode, config } = useWorkspace();
  const ModeIcon = config.icon;
  return <div className="sidebar-content">
    <Brand />
    <WorkspaceSwitcher onSwitch={close} />
    <p className="nav-caption">{config.subtitle}</p>
    <nav key={mode} className="workspace-navigation" aria-label={`${config.label} navigation`}>
      {config.navigation.map(({ to, label, icon: Icon }) => <NavLink key={to} to={to} end={to !== "/events"} onClick={close}><Icon aria-hidden="true" />{label}</NavLink>)}
    </nav>
    <button className="button secondary wide sidebar-create" onClick={() => { close(); primaryAction(); }}><FiPlus aria-hidden="true" />{config.action}</button>
    <div className="sidebar-bottom">
      <div className="sidebar-note"><ModeIcon className="note-symbol" aria-hidden="true" /><h3>{config.copy}</h3><p>{config.description}</p></div>
      <div className="profile-area"><span className="avatar" aria-hidden="true">{session?.username?.slice(0, 1).toUpperCase()}</span><div><strong>{session?.username}</strong><span>Your Circlo account</span></div><button className="icon-button" onClick={logout} aria-label="Log out" title="Log out"><FiLogOut /></button></div>
    </div>
  </div>;
}
