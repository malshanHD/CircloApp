import { lazy, Suspense, useState, useRef } from "react";
import { Link } from "react-router-dom";
import { FiChevronLeft, FiChevronRight, FiPlus, FiEdit2, FiTrash2 } from "react-icons/fi";
import { Page, ApiError, Empty, Modal, Skeleton, Success } from "../../components/common/UI";
import { usePersonalQuery, usePersonalMutation } from "../../features/personalExpenses/hooks";
import { personalExpenseService as service } from "../../services/personalExpenseService";
import { currentMonth, monthParams, moveMonth, money, dateLabel, paymentMethods } from "../../features/personalExpenses/format";
import { PersonalNav, QueryState, Stat } from "./PersonalShared";
import ExpenseForm from "./ExpenseForm";
const PersonalCharts = lazy(() => import("./PersonalCharts"));
export default function PersonalExpenses() {
  const [month, setMonth] = useState(currentMonth);
  const [filters, setFilters] = useState({});
  const [page, setPage] = useState(1);
  const [editing, setEditing] = useState(null);
  const [deleting, setDeleting] = useState(null);
  const [message, setMessage] = useState("");
  const deleteLock = useRef(false);
  const dashboard = usePersonalQuery("/personal-expenses/dashboard", monthParams(month));
  const categories = usePersonalQuery("/personal-expense-categories");
  const list = usePersonalQuery("/personal-expenses", { ...(filters.fromDate || filters.toDate ? {} : monthParams(month)), ...filters, page, pageSize: 10 });
  const remove = usePersonalMutation(service.remove, () => { setDeleting(null); setMessage("Expense deleted."); setPage(1); });
  const currency = dashboard.data?.currencyCode || "LKR";
  function changeMonth(value) { if (!/^\d{4}-\d{2}$/.test(value)) return; setMonth(value); setPage(1); }
  function apply(event) {
    event.preventDefault();
    const data = Object.fromEntries(new FormData(event.currentTarget));
    setFilters(data); setPage(1);
  }
  async function deleteExpense() {
    if (deleteLock.current) return;
    deleteLock.current = true;
    try { await remove.mutateAsync(deleting.id); } catch { /* Keep confirmation open on failure. */ }
    finally { deleteLock.current = false; }
  }
  return <Page className="personal-page">
    <div className="page-heading"><div><span className="eyebrow">YOUR MONEY, A LITTLE CLEARER</span><h1>Personal expenses</h1><p>Small details. A clearer picture.</p></div><button className="button primary" onClick={() => setEditing({})}><FiPlus /> Add expense</button></div>
    <PersonalNav />
    <div className="personal-month"><button className="icon-button" aria-label="Previous month" disabled={month <= "0001-01"} onClick={() => changeMonth(moveMonth(month, -1))}><FiChevronLeft /></button><label><span className="sr-only">Dashboard month</span><input type="month" min="0001-01" max="9998-12" value={month} onChange={e => changeMonth(e.target.value)} /></label><button className="icon-button" aria-label="Next month" disabled={month >= "9998-12"} onClick={() => changeMonth(moveMonth(month, 1))}><FiChevronRight /></button></div>
    {message && <Success>{message}</Success>}
    <QueryState query={dashboard}>{d => <div key={month} className="personal-month-content">
      <div className="summary-grid personal-summary">
        <Stat title="Total spent" note={`${d.expenseCount} expenses`}>{money(d.totalSpent, currency)}</Stat>
        <section className={`card personal-balance ${d.budgetStatus === "Exceeded" ? "over-budget" : ""}`}>
          <span className="eyebrow">Remaining this month</span><h2>{d.effectiveMonthlyLimit == null ? "No monthly limit set" : d.overBudgetAmount > 0 ? `Over budget by ${money(d.overBudgetAmount, currency)}` : money(d.remainingBalance, currency)}</h2>
          <p>{new Date(`${month}-01T12:00:00`).toLocaleDateString("en-GB", { month: "long", year: "numeric" })} · {d.budgetStatus.replace(/([a-z])([A-Z])/g, "$1 $2")}</p>
          {d.effectiveMonthlyLimit != null ? <><progress aria-label="Monthly budget used" max={100} value={d.percentageUsed == null ? (d.totalSpent > 0 ? 100 : 0) : Math.min(100, d.percentageUsed)} /><p>{d.percentageUsed == null ? "Percentage unavailable for a zero limit" : `${d.percentageUsed}% of budget used`}</p></> : <Link className="text-button" to={`/personal-expenses/settings?month=${month}`}>Set a monthly limit →</Link>}
        </section>
        <Stat title="Monthly spending limit" note={d.limitSource === "MonthlyOverride" ? "Custom limit for this month" : "Default monthly limit"}>{money(d.effectiveMonthlyLimit, currency)}<Link className="text-button small" to={`/personal-expenses/settings?month=${month}`}>Manage budget →</Link></Stat>
      </div>
      <div className="personal-small-stats"><span>Average per day: <strong>{money(d.averageDailySpending, currency)}</strong></span><span>Largest expense: <strong>{d.largestExpense ? `${d.largestExpense.description} · ${money(d.largestExpense.amount, currency)}` : "None yet"}</strong></span></div>
      <Suspense fallback={<Skeleton count={2} />}><PersonalCharts currency={currency} categories={d.categoryBreakdown} trend={d.dailySpendingTrend.map(t => ({ label: t.date.slice(8), total: t.total }))} /></Suspense>
      <section className="card personal-recent"><h2>Recent expenses this month</h2>{d.recentExpenses.length ? <ul className="personal-breakdown">{d.recentExpenses.map(e => <li key={e.id}><span>{e.description}<small>{dateLabel(e.expenseDate)} · {e.categoryName}</small></span><strong>{money(e.amount, currency)}</strong></li>)}</ul> : <p className="muted">Your next expense will appear here.</p>}</section>
    </div>}</QueryState>
    <section className="section-block"><h2>Your expense journal</h2><p className="small muted">Filters below apply to the journal; the overview above always shows the selected month.</p>
      <form className="card personal-filters" onSubmit={apply}>
        <label>Search<input name="search" maxLength={250} placeholder="Description or note" /></label>
        <label>Category<select name="categoryId"><option value="">All categories</option>{categories.data?.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}</select></label>
        <label>Payment method<select name="paymentMethod"><option value="">All methods</option>{paymentMethods.map(p => <option key={p}>{p}</option>)}</select></label>
        <label>Minimum amount<input name="minAmount" type="number" min="0" step="0.01" /></label><label>Maximum amount<input name="maxAmount" type="number" min="0" step="0.01" /></label>
        <label>From date<input name="fromDate" type="date" /></label><label>To date<input name="toDate" type="date" /></label>
        <label>Sort by<select name="sortBy"><option value="ExpenseDate">Expense date</option><option value="Amount">Amount</option><option value="Description">Description</option><option value="Category">Category</option><option value="CreatedAt">Date recorded</option></select></label>
        <label>Order<select name="sortDirection"><option value="desc">Descending</option><option value="asc">Ascending</option></select></label>
        <button className="button primary">Apply filters</button><button className="button secondary" type="reset" onClick={() => { setFilters({}); setPage(1); }}>Reset</button>
        {categories.isPending && <span role="status">Loading categories…</span>}{categories.isError && <ApiError error={categories.error} retry={() => categories.refetch()} />}
      </form>
      <QueryState query={list}>{data => data.items.length ? <><div className="card personal-list"><table><thead><tr><th>Expense</th><th>Date</th><th>Category / method</th><th>Amount</th><th>Actions</th></tr></thead><tbody>{data.items.map(e => <tr key={e.id}><th scope="row">{e.description}{e.note && <small>{e.note}</small>}</th><td data-label="Date">{dateLabel(e.expenseDate)}</td><td data-label="Category">{e.categoryName}<small>{e.paymentMethod || "Not specified"}</small></td><td data-label="Amount">{money(e.amount, currency)}</td><td><button className="icon-button" aria-label={`Edit ${e.description}`} onClick={() => setEditing(e)}><FiEdit2 /></button><button className="icon-button" aria-label={`Delete ${e.description}`} onClick={() => { remove.reset(); setDeleting(e); }}><FiTrash2 /></button></td></tr>)}</tbody></table></div><div className="personal-pagination"><button className="button secondary" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Previous</button><span>Page {data.page} of {data.totalPages} · {data.totalCount} expenses</span><button className="button secondary" disabled={page >= data.totalPages} onClick={() => setPage(p => p + 1)}>Next</button></div></> : <Empty title="A little space for your spending story." action={<button className="button primary" onClick={() => setEditing({})}>Add an expense</button>}>No expenses match this period and these filters.</Empty>}</QueryState>
    </section>
    {editing && <ExpenseForm expense={editing.id ? editing : null} onClose={() => setEditing(null)} onSaved={() => { setMessage(editing.id ? "Expense updated." : "Expense saved."); setEditing(null); setPage(1); }} />}
    {deleting && <Modal title="Delete this expense?" onClose={() => setDeleting(null)} busy={remove.isPending}><p><strong>{deleting.description}</strong> · {money(deleting.amount, currency)}</p><p className="muted small">It will be removed from your spending totals and insights.</p><ApiError error={remove.error} /><div className="form-actions"><button className="button secondary" disabled={remove.isPending} onClick={() => setDeleting(null)}>Keep expense</button><button className="button primary" disabled={remove.isPending} onClick={deleteExpense}>{remove.isPending ? "Deleting…" : "Delete expense"}</button></div></Modal>}
  </Page>;
}
