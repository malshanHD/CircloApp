import { useState, useEffect, useRef } from "react";
import { Outlet, useLocation } from "react-router-dom";
import { FiMenu, FiPlus } from "react-icons/fi";
import Navigation from "./NavigationBar";
import InviteNotifications from "../components/events/InviteNotifications";
import CreateEventModal from "../pages/events/CreateEventModal";
import { Modal } from "../components/common/UI";
export default function MainLayout() {
  const [drawer, setDrawer] = useState(false);
  const [create, setCreate] = useState(false);
  const location = useLocation();
  const main = useRef(null);
  useEffect(() => {
    main.current?.focus();
    window.scrollTo(0, 0);
  }, [location.pathname]);
  const section = location.pathname.startsWith("/events")
    ? "My events"
    : location.pathname.startsWith("/assistant")
      ? "Circlo AI"
      : "Overview";
  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">
        Skip to content
      </a>
      <aside className="desktop-sidebar">
        <Navigation create={() => setCreate(true)} />
      </aside>
      <div className="app-body">
        <header className="topbar">
          <div className="topbar-left">
            <button
              className="icon-button mobile-menu"
              aria-label="Open navigation"
              onClick={() => setDrawer(true)}
            >
              <FiMenu />
            </button>
            <span className="breadcrumb">
              Your workspace <span>/</span> <strong>{section}</strong>
            </span>
          </div>
          <span className="topbar-note">A little more together.</span>
          <InviteNotifications />
          <button
            className="button secondary topbar-create"
            onClick={() => setCreate(true)}
          >
            <FiPlus /> New event
          </button>
        </header>
        <main
          id="main-content"
          ref={main}
          tabIndex={-1}
          className="main-content"
        >
          <Outlet context={{ createEvent: () => setCreate(true) }} />
        </main>
        <footer className="app-footer">
          <span>circlo.</span> Shared plans. Clear expenses.
        </footer>
      </div>
      {drawer && (
        <Modal
          title="Your workspace"
          onClose={() => setDrawer(false)}
          className="nav-drawer"
        >
          <Navigation
            close={() => setDrawer(false)}
            create={() => setCreate(true)}
          />
        </Modal>
      )}
      {create && <CreateEventModal isOpen onClose={() => setCreate(false)} />}
    </div>
  );
}
