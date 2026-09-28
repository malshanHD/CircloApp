import { FiRepeat } from "react-icons/fi";
import { workspaces } from "./config";
import { useWorkspace } from "./WorkspaceContext";

export default function WorkspaceSwitcher({ onSwitch = () => {}, compact = false }) {
  const { mode, switchMode } = useWorkspace();
  const destination = mode === "personal" ? "groups" : "personal";
  const target = workspaces[destination];
  const Icon = target.icon;
  return <div className={`workspace-switcher${compact ? " compact" : ""}`}>
    <button type="button" aria-label={`Switch to ${target.label} workspace`} title={`Switch to ${target.label}`} onClick={() => { switchMode(destination); onSwitch(); }}>
      <Icon aria-hidden="true" />
      <span className="workspace-switch-label"><span>Switch to</span><strong>{target.label}</strong></span>
      <FiRepeat className="workspace-switch-arrow" aria-hidden="true" />
    </button>
  </div>;
}
