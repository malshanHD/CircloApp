import { useState, useEffect, useRef, lazy, Suspense } from "react";
import { Outlet, useLocation } from "react-router-dom";
import { FiMenu, FiPlus } from "react-icons/fi";
import Navigation from "./NavigationBar";
import InviteNotifications from "../components/events/InviteNotifications";
import CreateEventModal from "../pages/events/CreateEventModal";
import { Modal, Skeleton, Success } from "../components/common/UI";
import ThemeToggle from "../components/ThemeToggle";
import { WorkspaceProvider, useWorkspace } from "../features/workspace/WorkspaceContext";
import WorkspaceSwitcher from "../features/workspace/WorkspaceSwitcher";
import { workspaceSection } from "../features/workspace/config";
const ExpenseForm = lazy(() => import("../pages/personal/ExpenseForm"));

export default function MainLayout() {
  return <WorkspaceProvider><WorkspaceLayout /></WorkspaceProvider>;
}
function WorkspaceLayout() {
  const [drawer, setDrawer] = useState(false);
  const [create, setCreate] = useState(false);
  const [expense, setExpense] = useState(false);
  const [savedAt, setSavedAt] = useState(null);
  const location = useLocation();
  const { mode, config } = useWorkspace();
  const main = useRef(null);
  useEffect(() => {
    main.current?.focus();
    window.scrollTo(0, 0);
  }, [location.pathname]);
  const primaryAction = () => mode === "personal" ? setExpense(true) : setCreate(true);
  return <div className="app-shell" data-workspace={mode}>
    <a className="skip-link" href="#main-content">Skip to content</a>
    <aside className="desktop-sidebar"><Navigation primaryAction={primaryAction} /></aside>
    <div className="app-body">
      <header className="topbar">
        <div className="topbar-left">
          <button className="icon-button mobile-menu" aria-label={`Open ${config.label} navigation`} aria-expanded={drawer} aria-haspopup="dialog" onClick={() => setDrawer(true)}><FiMenu /></button>
          <span className="breadcrumb">{config.label}<span aria-hidden="true">/</span><strong>{workspaceSection(location.pathname, mode)}</strong></span>
          <div className="mobile-workspace"><WorkspaceSwitcher compact /></div>
        </div>
        <span className="topbar-note">{config.copy}</span>
        {mode === "personal" && location.pathname !== config.home && <button className="button secondary workspace-header-action" onClick={primaryAction}><FiPlus aria-hidden="true" />{config.action}</button>}
        <ThemeToggle />
        {mode === "groups" && <InviteNotifications />}
      </header>
      <main id="main-content" ref={main} tabIndex={-1} className="main-content">
        {savedAt === location.key && <Success>Expense saved.</Success>}
        <Suspense fallback={<Skeleton />}><Outlet context={{ createEvent: () => setCreate(true), addExpense: () => setExpense(true) }} /></Suspense>
      </main>
      <footer className="app-footer"><span>circlo.</span> {config.copy}</footer>
      <button className="button primary mobile-workspace-action" onClick={primaryAction}><FiPlus aria-hidden="true" />{config.action}</button>
    </div>
    {drawer && <Modal title={`${config.label} workspace`} onClose={() => setDrawer(false)} className="nav-drawer"><Navigation close={() => setDrawer(false)} primaryAction={primaryAction} /></Modal>}
    {create && mode === "groups" && <CreateEventModal isOpen onClose={() => setCreate(false)} />}
    {expense && mode === "personal" && <Suspense fallback={<Modal title="Add expense" onClose={() => setExpense(false)}><Skeleton count={1} /></Modal>}><ExpenseForm onClose={() => setExpense(false)} onSaved={() => { setExpense(false); setSavedAt(location.key); }} /></Suspense>}
  </div>;
}
