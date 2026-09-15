import { useState } from "react";
import {
  Link,
  useLocation,
  useParams,
  useSearchParams,
} from "react-router-dom";
import {
  FiUsers,
  FiPlus,
  FiCalendar,
  FiArrowLeft,
  FiZap,
} from "react-icons/fi";
import {
  useEvent,
  useSummary,
  isEventId,
  useAcceptInvite,
} from "../../features/events/hooks";
import {
  Page,
  ApiError,
  Skeleton,
  Success,
  Empty,
} from "../../components/common/UI";
import { formatAmount, formatDate } from "../../utils/format";
import AddMemberModal from "./AddMemberModal";
import AddExpense from "../expenses/AddExpense";
import EventAssistant from "../../components/ai/EventAssistant";
import EventExpenses from "../../components/events/EventExpenses";
function EventContent({ eventId }) {
  const query = useEvent(eventId);
  const summary = useSummary(eventId, query.isSuccess);
  const location = useLocation();
  const [params, setParams] = useSearchParams();
  const tab = ["overview", "expenses", "members", "ai"].includes(
    params.get("tab"),
  )
    ? params.get("tab")
    : "overview";
  const [invite, setInvite] = useState(false);
  const [expense, setExpense] = useState(null);
  const [success, setSuccess] = useState(location.state?.success || "");
  const accept = useAcceptInvite(eventId, () => {
    setSuccess("Invitation accepted. Welcome to your circle!");
    query.refetch();
  });
  if (query.isPending) return <Skeleton />;
  if (query.isError)
    return (
      <>
        <Link to="/events" className="text-button">
          ← All events
        </Link>
        <ApiError error={query.error} retry={() => query.refetch()} />
        <div className="card">
          <h2>Were you invited?</h2>
          <p className="muted small">
            If you have a pending invitation, accept it to view this event.
          </p>
          <ApiError error={accept.error} />
          <button
            className="button primary"
            onClick={() => accept.mutate()}
            disabled={accept.isPending}
          >
            {accept.isPending ? "Joining…" : "Accept invitation"}
          </button>
        </div>
      </>
    );
  const event = query.data;
  return (
    <Page>
      <Link to="/events" className="text-button inline-link">
        <FiArrowLeft /> All events
      </Link>
      {success && <Success>{success}</Success>}
      <div className="page-heading event-heading">
        <div>
          <span className="eyebrow">YOUR SHARED SPACE</span>
          <h1>{event.name}</h1>
          <p>{event.description}</p>
          <div className="detail-meta">
            <span>
              <FiCalendar /> {formatDate(event.createdAt)}
            </span>
            <span>
              <FiUsers /> {event.members.length} members
            </span>
          </div>
        </div>
        <button className="button secondary" onClick={() => setInvite(true)}>
          <FiUsers /> Invite someone
        </button>
      </div>
      <nav className="detail-tabs" aria-label="Event sections">
        {[
          ["overview", "Overview"],
          ["expenses", "Expenses"],
          ["members", "Members"],
          ["ai", "AI Assistant"],
        ].map(([value, label]) => (
          <button
            key={value}
            aria-current={tab === value ? "page" : undefined}
            className={tab === value ? "active" : ""}
            onClick={() => setParams({ tab: value }, { replace: true })}
          >
            {value === "ai" && <FiZap />}
            {label}
          </button>
        ))}
      </nav>
      {tab === "overview" && (
        <Page>
          {summary.isPending ? (
            <Skeleton />
          ) : summary.isError ? (
            <ApiError error={summary.error} retry={() => summary.refetch()} />
          ) : (
            <>
              <div className="summary-grid">
                <div className="card summary-card">
                  <span>Total event cost</span>
                  <strong>{formatAmount(summary.data?.totalCost)}</strong>
                  <p>Reported by Circlo</p>
                </div>
                <div className="card summary-card">
                  <span>Equal share per person</span>
                  <strong>
                    {formatAmount(summary.data?.equalSharePerPerson)}
                  </strong>
                  <p>Calculated by Circlo</p>
                </div>
                <div className="card summary-card">
                  <span>Your circle</span>
                  <strong>
                    {event.members.length}
                    <small> people</small>
                  </strong>
                  <p>Active event members</p>
                </div>
              </div>
              <p className="muted small amount-note">
                Amounts are shown as recorded. Currency is not specified.
              </p>
              <section className="card balances">
                <div className="section-heading">
                  <div>
                    <h2>Everyone, on the same page.</h2>
                    <p className="muted small">
                      Payments, settlements, and balances for this event.
                    </p>
                  </div>
                  <div className="form-actions">
                    <button
                      className="button secondary"
                      onClick={() => setExpense("settlement")}
                    >
                      Record settlement
                    </button>
                    <button
                      className="button primary"
                      onClick={() => setExpense("expense")}
                    >
                      <FiPlus /> Add expense
                    </button>
                  </div>
                </div>
                {!summary.data?.userBalances?.length ? (
                  <Empty title="No balances yet.">
                    Record an expense to get started.
                  </Empty>
                ) : (
                  <div className="table-scroll">
                    <table>
                      <caption className="sr-only">
                        Member balances reported by Circlo
                      </caption>
                      <thead>
                        <tr>
                          <th scope="col">Member</th>
                          <th scope="col">Paid</th>
                          <th scope="col">Settled</th>
                          <th scope="col">Received</th>
                          <th scope="col">Balance</th>
                        </tr>
                      </thead>
                      <tbody>
                        {summary.data.userBalances.map((member) => (
                          <tr key={member.userId}>
                            <th scope="row">{member.userName}</th>
                            <td>{formatAmount(member.totalPaid)}</td>
                            <td>{formatAmount(member.totalSettled)}</td>
                            <td>{formatAmount(member.totalSettledReceived)}</td>
                            <td>
                              <span
                                className={
                                  member.balance > 0
                                    ? "balance-positive"
                                    : member.balance < 0
                                      ? "balance-negative"
                                      : ""
                                }
                              >
                                {member.status || formatAmount(member.balance)}
                              </span>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </section>
            </>
          )}
        </Page>
      )}
      {tab === "expenses" && (
        <EventExpenses eventId={eventId} onAdd={() => setExpense("expense")} />
      )}
      {tab === "members" && (
        <Page className="card">
          <div className="section-heading">
            <div>
              <h2>The people behind the plan.</h2>
              <p className="muted small">
                {event.members.length} active members
              </p>
            </div>
            <button
              className="button secondary"
              onClick={() => setInvite(true)}
            >
              Invite someone
            </button>
          </div>
          <div className="members-grid">
            {event.members.map((member) => (
              <div className="member-card" key={member.userId}>
                <span className="avatar">{member.fullName.slice(0, 1)}</span>
                <div>
                  <h3>{member.fullName}</h3>
                  <p>@{member.username}</p>
                </div>
                <span className="badge">{member.role}</span>
              </div>
            ))}
          </div>
        </Page>
      )}
      <div hidden={tab !== "ai"}>
        <EventAssistant
          key={eventId}
          eventId={eventId}
          eventName={event.name}
        />
      </div>
      {invite && (
        <AddMemberModal
          isOpen
          eventId={eventId}
          eventName={event.name}
          onClose={() => setInvite(false)}
        />
      )}
      {expense && (
        <AddExpense
          eventId={eventId}
          settlement={expense === "settlement"}
          onClose={() => setExpense(null)}
          onSuccess={setSuccess}
        />
      )}
    </Page>
  );
}
export default function EventDetails() {
  const { eventId } = useParams();
  return isEventId(eventId) ? (
    <EventContent key={eventId} eventId={eventId} />
  ) : (
    <ApiError error={new Error("This event link is invalid.")} />
  );
}
