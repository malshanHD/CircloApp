import { useState } from "react";
import { FiPlus, FiRefreshCw, FiSearch } from "react-icons/fi";
import { useEventExpenses } from "../../features/events/hooks";
import { formatAmount, formatDate } from "../../utils/format";
import { Page, ApiError, Empty, Skeleton } from "../common/UI";

export default function EventExpenses({ eventId, onAdd }) {
  const query = useEventExpenses(eventId);
  const [search, setSearch] = useState("");
  const [type, setType] = useState("all");
  const rows = query.data || [];
  const filtered = rows.filter(
    (row) =>
      (type === "all" || String(row.type) === type) &&
      `${row.description || ""} ${row.paidUser}`
        .toLowerCase()
        .includes(search.trim().toLowerCase()),
  );
  return (
    <Page className="card expense-ledger">
      <div className="section-heading">
        <div>
          <h2>Every expense, in one place.</h2>
          <p className="muted small">
            Payments and settlements recorded for this event.
          </p>
        </div>
        <button className="button primary" onClick={onAdd}>
          <FiPlus /> Add expense
        </button>
      </div>
      <div className="expense-toolbar">
        <label className="search-input">
          <FiSearch />
          <span className="sr-only">Search event expenses</span>
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search description or payer…"
          />
        </label>
        <label className="transaction-filter">
          <span className="sr-only">Transaction type</span>
          <select value={type} onChange={(e) => setType(e.target.value)}>
            <option value="all">All transactions</option>
            <option value="1">Expenses</option>
            <option value="2">Settlements</option>
          </select>
        </label>
        <button
          className="icon-button"
          aria-label="Refresh event expenses"
          disabled={query.isFetching}
          onClick={() => query.refetch()}
        >
          <FiRefreshCw />
        </button>
      </div>
      {query.isPending ? (
        <Skeleton count={1} />
      ) : query.isError ? (
        <ApiError error={query.error} retry={() => query.refetch()} />
      ) : !rows.length ? (
        <Empty
          title="No expenses yet."
          action={
            <button className="button primary" onClick={onAdd}>
              Add the first expense
            </button>
          }
        >
          Record a payment to start keeping track together.
        </Empty>
      ) : !filtered.length ? (
        <Empty
          title="No matching transactions."
          action={
            <button
              className="button secondary"
              onClick={() => {
                setSearch("");
                setType("all");
              }}
            >
              Clear filters
            </button>
          }
        >
          Try another description, payer, or transaction type.
        </Empty>
      ) : (
        <div className="table-scroll">
          <table className="expense-table">
            <caption className="sr-only">
              Expenses and settlements for this event
            </caption>
            <thead>
              <tr>
                <th scope="col">Description</th>
                <th scope="col">Paid by</th>
                <th scope="col">Date</th>
                <th scope="col">Type</th>
                <th scope="col">Amount</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((row, index) => (
                <tr key={index}>
                  <th scope="row">{row.description || "No description"}</th>
                  <td>{row.paidUser}</td>
                  <td>
                    <time dateTime={row.dateAndTime}>
                      {formatDate(row.dateAndTime)}
                    </time>
                  </td>
                  <td>
                    <span className={`badge transaction-${row.type}`}>
                      {row.type === 1
                        ? "Expense"
                        : row.type === 2
                          ? "Settlement"
                          : "Transaction"}
                    </span>
                  </td>
                  <td className="expense-amount">{formatAmount(row.amount)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {query.isSuccess && rows.length > 0 && (
        <p className="muted small amount-note">
          Showing {filtered.length} of {rows.length} transactions. Amounts are
          shown as recorded; currency is not specified.
        </p>
      )}
    </Page>
  );
}
