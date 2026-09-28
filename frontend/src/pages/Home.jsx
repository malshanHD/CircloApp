import { Link } from "react-router-dom";
import { FiArrowRight, FiPieChart, FiUsers } from "react-icons/fi";
import { Page } from "../components/common/UI";
export default function Home() {
  return <Page><div className="page-heading"><div><span className="eyebrow">WELCOME TO YOUR CIRCLE</span><h1>A little clarity.<br />A lot more living.</h1><p>Your own spending or your next shared plan. Where shall we start?</p></div></div>
    <div className="personal-choices"><section className="card personal-choice"><span className="empty-icon"><FiPieChart /></span><span className="eyebrow">PERSONAL EXPENSES</span><h2>Understand where your money goes.</h2><p>Set a monthly budget, organize spending into categories, and discover the little patterns that help you plan ahead.</p><Link className="button primary" to="/personal-expenses">Track my expenses <FiArrowRight /></Link></section>
    <section className="card personal-choice group-choice"><span className="empty-icon"><FiUsers /></span><span className="eyebrow">GROUP PLANS</span><h2>Keep shared plans and expenses together.</h2><p>Bring your people together, plan your next event, and keep shared expenses clear from the first idea to the last settlement.</p><Link className="button primary" to="/dashboard">Plan with a group <FiArrowRight /></Link></section></div>
  </Page>;
}
