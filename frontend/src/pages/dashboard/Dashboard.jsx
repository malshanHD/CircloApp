import { lazy, Suspense, useState } from "react";
import { Link, useOutletContext } from "react-router-dom";
import { FiArrowRight, FiPlus, FiZap, FiArrowUpRight } from "react-icons/fi";
import { useAuth } from "../../hooks/useAuth";
import { useEvents, useExpenseHistory } from "../../features/events/hooks";
import { Page, ApiError, Skeleton, Empty } from "../../components/common/UI";
import EventCard from "../../components/events/EventCard";
const ExpenseCharts = lazy(() => import("./ExpenseCharts"));
export default function Dashboard() {
  const { session } = useAuth();
  const { createEvent } = useOutletContext();
  const query = useEvents(1, 3);
  const [year, setYear] = useState(new Date().getFullYear());
  const history = useExpenseHistory(year);
  return (
    <Page>
      <div className="page-heading">
        <div>
          <span className="eyebrow">YOUR EVERYDAY, A LITTLE LIGHTER</span>
          <h1>
            Hey {session?.username}, welcome back{" "}
            <span className="hello-dot">✳</span>
          </h1>
          <p>Good times ahead. Let's keep the details simple.</p>
        </div>
        <button className="button primary" onClick={createEvent}>
          <FiPlus /> Create event
        </button>
      </div>
      <section className="dashboard-hero">
        <div>
          <span className="eyebrow">MORE MEMORIES. LESS MATH.</span>
          <h2>
            Big plans start
            <br />
            with a little circle.
          </h2>
          <p>
            Bring your people together. Keep shared expenses
            <br className="desktop-break" /> clear, from the first plan to the
            last goodbye.
          </p>
          <button className="button dark-button" onClick={createEvent}>
            Start a new event <FiArrowUpRight />
          </button>
        </div>
        <div className="hero-art" aria-hidden="true">
          <span className="hero-orbit one" />
          <span className="hero-orbit two" />
          <span className="hero-circle c1">Together</span>
          <span className="hero-circle c2">
            <FiArrowUpRight />
          </span>
          <span className="hero-circle c3">✳</span>
          <span className="hero-caption">A shared space for every plan.</span>
        </div>
      </section>
      <section className="section-block">
        <div className="section-heading">
          <div>
            <h2>
              Recent events{" "}
              {query.data && (
                <span className="count-pill">{query.data.totalCount}</span>
              )}
            </h2>
            <p className="muted small">Pick up where your circle left off.</p>
          </div>
          <Link className="text-button inline-link" to="/events">
            View all events <FiArrowRight />
          </Link>
        </div>
        {query.isPending ? (
          <Skeleton />
        ) : query.isError ? (
          <ApiError error={query.error} retry={() => query.refetch()} />
        ) : query.data?.items.length ? (
          <div className="event-grid">
            {query.data.items.map((event, index) => (
              <EventCard key={event.id} event={event} index={index} />
            ))}
          </div>
        ) : (
          <Empty
            title="Make your first plan."
            action={
              <button className="button primary" onClick={createEvent}>
                <FiPlus /> Create an event
              </button>
            }
          >
            Trips, dinners, everyday adventures. Give your group a place to keep
            it all together.
          </Empty>
        )}
      </section>
      <section className="ai-banner">
        <span className="ai-icon">
          <FiZap />
        </span>
        <div>
          <span className="eyebrow">MEET CIRCLO AI</span>
          <h2>A question away from clarity.</h2>
          <p>Ask about your event's expenses, in your own words.</p>
        </div>
        <Link
          to={
            query.data?.items?.[0]
              ? `/events/${query.data.items[0].id}?tab=ai`
              : "/assistant"
          }
          className="button secondary"
        >
          Let's ask Circlo <FiArrowUpRight />
        </Link>
      </section>
      <section className="section-block">
        <div className="section-heading">
          <div>
            <h2>Your expense picture</h2>
            <p className="muted small">
              Amounts as recorded. Currency is not specified.
            </p>
          </div>
          <label className="year-picker">
            Year{" "}
            <input
              aria-label="Expense year"
              type="number"
              min="1"
              max="9999"
              value={year}
              onChange={(e) => {
                const value = Number(e.target.value);
                if (value >= 1 && value <= 9999) setYear(value);
              }}
            />
          </label>
        </div>
        <div className="chart-grid">
          {[
            ["By event", history.events, "events"],
            ["Month by month", history.months, "monthly"],
          ].map(([title, result, kind]) => (
            <div className="card chart-card" key={kind}>
              <h3>{title}</h3>
              {result.isPending ? (
                <Skeleton count={1} />
              ) : result.isError ? (
                <ApiError error={result.error} retry={() => result.refetch()} />
              ) : !result.data?.length ||
                result.data.every(
                  (item) => (item.totalExpenses ?? item.totalAmount) === 0,
                ) ? (
                <div className="chart-empty">
                  <p>No expenses to show yet.</p>
                  <span>Once recorded, your expenses will appear here.</span>
                </div>
              ) : (
                <Suspense fallback={<Skeleton count={1} />}>
                  <ExpenseCharts data={result.data} kind={kind} />
                </Suspense>
              )}
            </div>
          ))}
        </div>
      </section>
    </Page>
  );
}
