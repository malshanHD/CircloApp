import { useNavigate, useSearchParams } from "react-router-dom";
import { useAcceptInvite, isEventId } from "../../features/events/hooks";
import { Page, ApiError } from "../../components/common/UI";
export default function AcceptInvitation() {
  const [params] = useSearchParams();
  const eventId = params.get("eventId");
  const navigate = useNavigate();
  const mutation = useAcceptInvite(eventId, () =>
    navigate(`/events/${eventId}`, {
      replace: true,
      state: { success: "You're in. Welcome to the event!" },
    }),
  );
  return (
    <Page className="card">
      <h1>Join the circle.</h1>
      <p className="subtitle">
        Accept your invitation to start sharing this event.
      </p>
      {!isEventId(eventId) ? (
        <p role="alert">This invitation link has an invalid event ID.</p>
      ) : (
        <>
          <ApiError error={mutation.error} />
          <button
            className="button primary"
            disabled={mutation.isPending}
            onClick={() => mutation.mutate()}
          >
            {mutation.isPending ? "Joining…" : "Accept invitation"}
          </button>
        </>
      )}
    </Page>
  );
}
