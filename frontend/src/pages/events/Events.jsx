import { useState } from "react";
import { useLocation, useOutletContext } from "react-router-dom";
import { FiPlus, FiSearch, FiRefreshCw, FiZap } from "react-icons/fi";
import { useEvents } from "../../features/events/hooks";
import {
  Page,
  ApiError,
  Skeleton,
  Empty,
  Success,
} from "../../components/common/UI";
import EventCard from "../../components/events/EventCard";
export default function Events({ assistant = false }) {
  const { createEvent } = useOutletContext();
  const location = useLocation();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const query = useEvents(page);
  const events = query.data?.items || [];
  const visible = events.filter((event) =>
    `${event.name} ${event.description || ""}`
      .toLowerCase()
      .includes(search.toLowerCase()),
  );
  return (
    <Page>
      <div className="page-heading">
        <div>
          <span className="eyebrow">
            {assistant
              ? "A LITTLE CLARITY GOES A LONG WAY"
              : "PLANS WORTH SHARING"}
          </span>
          <h1>
            {assistant
              ? "Meet your expense assistant."
              : "Your plans. Your people."}
          </h1>
          <p>
            {assistant
              ? "Choose an event, then open its AI Assistant to ask about expenses."
              : "All your shared events, in one happy place."}
          </p>
        </div>
        <button className="button primary" onClick={createEvent}>
          <FiPlus /> Create event
        </button>
      </div>
      {location.state?.success && <Success>{location.state.success}</Success>}
      {assistant && (
        <div className="info-banner">
          <FiZap />
          <p>
            Every conversation stays with its event. Ask a question and Circlo
            AI will use that event's expense data.
          </p>
        </div>
      )}
      <div className="section-heading events-toolbar">
        <h2>
          My events{" "}
          {query.data && (
            <span className="count-pill">{query.data.totalCount}</span>
          )}
        </h2>
        <div className="toolbar-actions">
          <label className="search-input">
            <FiSearch />
            <span className="sr-only">Filter events on this page</span>
            <input
              placeholder="Search this page…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </label>
          <button
            className="icon-button"
            aria-label="Refresh events"
            disabled={query.isFetching}
            onClick={() => query.refetch()}
          >
            <FiRefreshCw />
          </button>
        </div>
      </div>
      {query.isPending ? (
        <Skeleton />
      ) : query.isError ? (
        <ApiError error={query.error} retry={() => query.refetch()} />
      ) : visible.length ? (
        <div className="event-grid">
          {visible.map((event, i) => (
            <EventCard key={event.id} event={event} index={i} />
          ))}
        </div>
      ) : (
        <Empty
          title={
            search
              ? "No matching plans on this page."
              : "Your next memory starts here."
          }
          action={
            <button
              className="button primary"
              onClick={search ? () => setSearch("") : createEvent}
            >
              {search ? "Clear search" : "Create your first event"}
            </button>
          }
        >
          {search
            ? "Try another name, clear your search, or browse another page."
            : "Create an event, invite your people, and keep shared expenses simple."}
        </Empty>
      )}
      {query.data?.totalPages > 1 && (
        <div className="pagination">
          <button
            className="button secondary"
            disabled={page === 1 || query.isFetching}
            onClick={() => setPage(page - 1)}
          >
            Previous
          </button>
          <span>
            Page {page} of {query.data.totalPages}
          </span>
          <button
            className="button secondary"
            disabled={page >= query.data.totalPages || query.isFetching}
            onClick={() => setPage(page + 1)}
          >
            Next
          </button>
        </div>
      )}
    </Page>
  );
}
