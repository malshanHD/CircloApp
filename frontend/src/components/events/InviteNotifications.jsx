import { useState } from "react";
import { useIsMutating } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { FiBell, FiRefreshCw, FiUserPlus } from "react-icons/fi";
import {
  useJoinRequests,
  useJoinAction,
  isEventId,
} from "../../features/events/hooks";
import { Modal, ApiError, Empty, Skeleton } from "../common/UI";

function Invitation({ invitation, onAccepted }) {
  const mutation = useJoinAction(
    invitation.eventId,
    onAccepted,
    invitation.userId,
  );
  return (
    <li className="invitation-item">
      <span className="invitation-icon" aria-hidden="true">
        <FiUserPlus />
      </span>
      <div className="invitation-content">
        <p>
          <strong>{invitation.fullName}</strong> requested to join{" "}
          <strong>{invitation.eventName}</strong>.
        </p>
        <p className="muted small">
          @{invitation.username} · Approve to give this user access to the
          event.
        </p>
        <ApiError error={mutation.error} />
        <button
          className="button primary"
          aria-label={`Approve ${invitation.fullName} for ${invitation.eventName}`}
          disabled={mutation.isPending || !isEventId(invitation.eventId)}
          onClick={() => {
            if (!mutation.isPending) mutation.mutate();
          }}
        >
          {mutation.isPending ? "Approving…" : "Approve request"}
        </button>
      </div>
    </li>
  );
}

export default function InviteNotifications() {
  const [open, setOpen] = useState(false);
  const query = useJoinRequests();
  const navigate = useNavigate();
  const accepting = useIsMutating({ mutationKey: ["join-action"] }) > 0;
  const count = query.data?.length;
  const label = query.isPending
    ? "Join requests, loading"
    : query.isError
      ? "Join requests, unable to refresh"
      : `Join requests, ${count ?? 0} pending`;
  return (
    <>
      <button
        className="icon-button notifications-button"
        aria-label={label}
        aria-haspopup="dialog"
        aria-expanded={open}
        onClick={() => setOpen(true)}
      >
        <FiBell />
        {typeof count === "number" && count > 0 && (
          <span className="notification-count" aria-hidden="true">
            {count > 99 ? "99+" : count}
          </span>
        )}
        {query.isError && (
          <span className="notification-warning" aria-hidden="true">
            !
          </span>
        )}
      </button>
      <span className="sr-only" role="status">
        {typeof count === "number"
          ? `${count} pending join request${count === 1 ? "" : "s"}`
          : ""}
      </span>
      {open && (
        <Modal
          title="Join requests"
          onClose={() => setOpen(false)}
          busy={accepting}
          className="invitations-modal"
        >
          <div className="section-heading">
            <p className="muted small">
              {typeof count === "number"
                ? `${count} request${count === 1 ? "" : "s"} waiting for approval.`
                : "Your next shared plan could be here."}
            </p>
            <button
              className="icon-button"
              aria-label="Refresh join requests"
              disabled={query.isFetching || accepting}
              onClick={() => query.refetch()}
            >
              <FiRefreshCw />
            </button>
          </div>
          {query.isPending ? (
            <Skeleton count={1} />
          ) : (
            <>
              <ApiError error={query.error} retry={() => query.refetch()} />
              {query.data?.length ? (
                <ul className="invitation-list">
                  {query.data.map((invitation) => (
                    <Invitation
                      key={`${invitation.eventId}-${invitation.userId}`}
                      invitation={invitation}
                      onAccepted={() => {
                        setOpen(false);
                        navigate(`/events/${invitation.eventId}`, {
                          state: {
                            success: `${invitation.fullName} has been approved.`,
                          },
                        });
                      }}
                    />
                  ))}
                </ul>
              ) : (
                !query.isError && (
                  <Empty title="You're all caught up.">
                    Requests to join events you administer will appear here.
                  </Empty>
                )
              )}
            </>
          )}
        </Modal>
      )}
    </>
  );
}
