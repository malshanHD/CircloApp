import { Link } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { money } from "../../features/personalExpenses/format";

export default function BalanceCard({ data, month, currency }) {
  const { session } = useAuth();
  const limit = data.effectiveMonthlyLimit;
  const usage = limit == null ? null : limit > 0 ? data.totalSpent / limit * 100 : data.totalSpent > 0 ? Infinity : 0;
  const tone = usage == null ? "neutral" : usage > 100 ? "risk" : usage >= 80 ? "warning" : usage >= 50 ? "steady" : "healthy";
  const labels = { neutral: "No limit set", healthy: "Room to spend", steady: "Keeping pace", warning: "Close to your limit", risk: "Over budget" };
  const end = new Date(`${month}-01T12:00:00`);
  end.setMonth(end.getMonth() + 1, 0);
  const endLabel = end.toLocaleDateString("en-GB", { day: "2-digit", month: "short", year: "numeric" });
  return <section className="card personal-balance budget-card" data-tone={tone} aria-label="Monthly budget balance">
    <div className="budget-card-top"><span className="budget-card-brand">circlo<span>.</span></span><span className="budget-card-status">{labels[tone]}</span></div>
    <div className="budget-card-amount"><span className="eyebrow">{usage > 100 ? "Over budget by" : "Remaining this month"}</span><h2>{limit == null ? "Your budget starts here" : money(usage > 100 ? data.overBudgetAmount : data.remainingBalance, currency)}</h2></div>
    {limit != null ? <div className="budget-card-usage"><progress aria-label="Monthly budget used" max={100} value={Math.min(100, usage)} /><p>{Number.isFinite(usage) ? `${data.percentageUsed ?? 0}% of budget used` : "Spending exceeds your zero limit"}</p></div> : <Link className="text-button" to={`/personal-expenses/settings?month=${month}`}>Set a monthly limit →</Link>}
    <div className="budget-card-details"><div><span>Budget for</span><strong>{session?.username || "You"}</strong></div><div><span>Budget period ends</span><strong>{endLabel}</strong></div></div>
  </section>;
}
