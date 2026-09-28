import { lazy, Suspense, useState } from "react";
import { Page, Empty, Skeleton } from "../../components/common/UI";
import { usePersonalQuery } from "../../features/personalExpenses/hooks";
import { currentMonth, localDate, money, dateLabel } from "../../features/personalExpenses/format";
import { PersonalNav, QueryState, Stat } from "./PersonalShared";
const PersonalCharts = lazy(() => import("./PersonalCharts"));
export default function PersonalAnalysis() {
  const [range, setRange] = useState({ fromDate: `${currentMonth()}-01`, toDate: localDate() });
  const query = usePersonalQuery("/personal-expenses/analysis", range);
  function submit(e) { e.preventDefault(); setRange(Object.fromEntries(new FormData(e.currentTarget))); }
  return <Page className="personal-page"><div className="page-heading"><div><span className="eyebrow">SEE THE BIGGER PICTURE</span><h1>Your spending insights</h1><p>Clear numbers. Useful patterns. Entirely your own.</p></div></div><PersonalNav />
    <form className="card personal-analysis-range" onSubmit={submit}><label>From date<input name="fromDate" type="date" required defaultValue={range.fromDate} /></label><label>To date<input name="toDate" type="date" required defaultValue={range.toDate} /></label><button className="button primary">Explore spending</button></form>
    <QueryState query={query}>{d => <>
      <p className="small muted personal-period">{dateLabel(d.fromDate)} – {dateLabel(d.toDate)} · {d.expenseCount} expenses</p>
      <div className="summary-grid"><Stat title="Total spending">{money(d.totalSpent, d.currencyCode)}</Stat><Stat title="Average expense">{money(d.averageExpenseAmount, d.currencyCode)}</Stat><Stat title="Highest expense" note={d.highestExpense?.description}>{money(d.highestExpense?.amount, d.currencyCode)}</Stat></div>
      <section className="card personal-comparison"><h2>A little perspective</h2><p>The preceding equal-length period: <strong>{money(d.previousPeriodTotal, d.currencyCode)}</strong>.</p><p>{d.percentageChange == null ? "No spending in the previous period, so a percentage comparison is not available." : `${Math.abs(d.percentageChange)}% ${d.percentageChange >= 0 ? "more" : "less"} spending than the preceding period.`}</p><p className="small muted">Highest-spending category: {d.highestSpendingCategory ? `${d.highestSpendingCategory.name} · ${money(d.highestSpendingCategory.total, d.currencyCode)}` : "None yet"}</p><p className="small muted">Highest-spending day: {d.highestSpendingDay ? `${dateLabel(d.highestSpendingDay.date)} · ${money(d.highestSpendingDay.total, d.currencyCode)}` : "None yet"}</p></section>
      {d.expenseCount ? <Suspense fallback={<Skeleton count={2} />}><PersonalCharts categories={d.categoryTotals} currency={d.currencyCode} trendTitle="Monthly spending" trend={d.monthlyTotals.map(m => ({ label: `${m.year}-${String(m.month).padStart(2, "0")}`, total: m.total }))} /></Suspense> : <Empty title="Your spending story starts with one expense.">There are no expenses in this range. Try another period or add your first expense.</Empty>}
    </>}</QueryState>
  </Page>;
}
