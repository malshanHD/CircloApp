import { useEffect, useRef } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useJoinAction, isEventId } from "../../features/events/hooks";
import { eventService } from "../../services/eventService";
import { Page, ApiError, Skeleton } from "../../components/common/UI";
export default function AcceptInvitation() {
  const [params] = useSearchParams();
  const eventId = params.get("eventId");
  const query = useQuery({
    queryKey: ["join-status", eventId],
    queryFn: ({ signal }) => eventService.getJoinStatus(eventId, signal),
    enabled: isEventId(eventId),
    retry: false,
    refetchInterval: 15000,
  });
  const mutation = useJoinAction(eventId, () => query.refetch());
  const attemptedEvent = useRef(null);
  useEffect(() => {
    if (
      query.data?.status === "not-requested" &&
      attemptedEvent.current !== eventId
    ) {
      attemptedEvent.current = eventId;
      mutation.mutate();
    }
  }, [eventId, query.data?.status, mutation]);
  if (!isEventId(eventId))
    return (
      <Page className="card">
        <ApiError error={new Error("This event link is invalid.")} />
      </Page>
    );
  const status = query.data?.status;
  return (
    <Page className="card">
      <h1>{query.data?.eventName || "Join the circle."}</h1>
      {query.isPending ? (
        <Skeleton count={1} />
      ) : query.isError ? (
        <ApiError error={query.error} retry={() => query.refetch()} />
      ) : (
        <>
          {status === "active" ? (
            <>
              <p>You are an approved member of this event.</p>
              <Link className="button primary" to={`/events/${eventId}`}>
                Open event
              </Link>
            </>
          ) : status === "pending" ? (
            <>
              <p role="status">Your request is waiting for admin approval.</p>
              <p className="muted small">
                The event will become available once the admin approves you.
              </p>
              <button
                className="button secondary"
                disabled={query.isFetching}
                onClick={() => query.refetch()}
              >
                Check approval status
              </button>
            </>
          ) : (
            <>
              <p>
                Your signed-in Circlo account is requesting access to this
                event. The admin must approve you before you can see members or
                expenses.
              </p>
              <ApiError error={mutation.error} />
              {mutation.isError ? (
                <button
                  className="button primary"
                  disabled={!mutation.isError || mutation.isPending}
                  onClick={() => {
                    if (!mutation.isPending) mutation.mutate();
                  }}
                >
                  Retry join request
                </button>
              ) : (
                <p role="status">Sending join request…</p>
              )}
            </>
          )}
        </>
      )}
    </Page>
  );
}
