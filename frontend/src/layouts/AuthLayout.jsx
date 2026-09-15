import { Brand, Page } from "../components/common/UI";
import { FiArrowUpRight, FiUsers, FiZap, FiCheck } from "react-icons/fi";
export default function AuthLayout({ title, subtitle, children }) {
  return (
    <div className="auth-layout">
      <aside className="auth-story">
        <Brand />
        <div className="story-content">
          <span className="eyebrow">GOOD TIMES. SHARED FAIRLY.</span>
          <h1>
            Make memories.
            <br />
            Leave the math
            <br />
            <em>to Circlo.</em>
          </h1>
          <p>
            From weekend escapes to everyday plans, keep your group's expenses
            together.
          </p>
          <div className="story-illustration" aria-hidden="true">
            <div className="orbit orbit-one" />
            <div className="orbit orbit-two" />
            <span className="illustration-center">
              <FiUsers />
            </span>
            <span className="floating-label label-one">
              <FiCheck /> A little more together
            </span>
            <span className="floating-label label-two">
              <FiZap /> A little less figuring out
            </span>
            <span className="orbit-arrow">
              <FiArrowUpRight />
            </span>
          </div>
        </div>
        <p className="story-footer">
          Your people. Your plans. One shared space.
        </p>
      </aside>
      <main className="auth-main">
        <div className="mobile-brand">
          <Brand />
        </div>
        <Page className="auth-form">
          <span className="eyebrow">WELCOME TO YOUR CIRCLE</span>
          <h1>{title}</h1>
          <p className="subtitle">{subtitle}</p>
          {children}
        </Page>
        <p className="auth-footer">Less keeping track. More being there.</p>
      </main>
    </div>
  );
}
