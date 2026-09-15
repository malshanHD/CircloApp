import { useState } from "react";
import { useIsMutating } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { FiBell, FiRefreshCw, FiUserPlus } from "react-icons/fi";
import {
  useInvitations,
  useAcceptInvite,
  isEventId,
} from "../../features/events/hooks";
import { Modal, ApiError, Empty, Skeleton } from "../common/UI";

function Invitation({ invitation, onAccepted }) {
  const mutation = useAcceptInvite(invitation.eventId, onAccepted);
  return (
    <li className="invitation-item">
      <span className="invitation-icon" aria-hidden="true">
        <FiUserPlus />
      </span>
      <div className="invitation-content">
        <p>
          <strong>{invitation.inviterName}</strong> invited you to{" "}
          <strong>{invitation.eventName}</strong>.
        </p>
        <p className="muted small">
          Join the event to view its members and shared expenses.
        </p>
        <ApiError error={mutation.error} />
        <button
          className="button primary"
          aria-label={`Accept invitation to ${invitation.eventName}`}
          disabled={mutation.isPending || !isEventId(invitation.eventId)}
          onClick={() => {
            if (!mutation.isPending) mutation.mutate();
          }}
        >
          {mutation.isPending ? "Joining…" : "Accept invitation"}
        </button>
      </div>
    </li>
  );
}

export default function InviteNotifications() {
  const [open, setOpen] = useState(false);
  const query = useInvitations();
  const navigate = useNavigate();
  const accepting = useIsMutating({ mutationKey: ["accept-invite"] }) > 0;
  const count = query.data?.invitationsCount;
  const label = query.isPending
    ? "Invitations, loading"
    : query.isError
      ? "Invitations, unable to refresh"
      : `Invitations, ${count ?? 0} pending`;
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
          ? `${count} pending event invitation${count === 1 ? "" : "s"}`
          : ""}
      </span>
      {open && (
        <Modal
          title="Your invitations"
          onClose={() => setOpen(false)}
          busy={accepting}
          className="invitations-modal"
        >
          <div className="section-heading">
            <p className="muted small">
              {typeof count === "number"
                ? `${count} invitation${count === 1 ? "" : "s"} waiting for you.`
                : "Your next shared plan could be here."}
            </p>
            <button
              className="icon-button"
              aria-label="Refresh invitations"
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
              {query.data?.inviteDetails?.length ? (
                <ul className="invitation-list">
                  {query.data.inviteDetails.map((invitation) => (
                    <Invitation
                      key={invitation.eventId}
                      invitation={invitation}
                      onAccepted={() => {
                        setOpen(false);
                        navigate(`/events/${invitation.eventId}`, {
                          state: {
                            success: `You've joined ${invitation.eventName}.`,
                          },
                        });
                      }}
                    />
                  ))}
                </ul>
              ) : (
                !query.isError && (
                  <Empty title="You're all caught up.">
                    When someone invites you to an event, it will appear here.
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
