import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { userService } from "../../services/userService";
import { useInvite } from "../../features/events/hooks";
import { Modal, ApiError, Success } from "../../components/common/UI";
export default function AddMemberModal({
  isOpen,
  onClose,
  eventName,
  eventId,
}) {
  const [search, setSearch] = useState("");
  const [term, setTerm] = useState("");
  const [selected, setSelected] = useState("");
  const [success, setSuccess] = useState(false);
  useEffect(() => {
    const timer = setTimeout(() => setTerm(search.trim()), 300);
    return () => clearTimeout(timer);
  }, [search]);
  const query = useQuery({
    queryKey: ["searchUsers", term],
    queryFn: ({ signal }) => userService.searchUsers(term, signal),
    enabled: isOpen && term.length >= 2,
  });
  const mutation = useInvite(eventId, () => setSuccess(true));
  if (!isOpen) return null;
  return (
    <Modal
      title="Bring your people."
      onClose={onClose}
      busy={mutation.isPending}
    >
      <p className="muted small">Invite a member to {eventName}.</p>
      {success ? (
        <>
          <Success>
            Invitation created. Your friend can join after accepting it.
          </Success>
          <button className="button primary wide" onClick={onClose}>
            Done
          </button>
        </>
      ) : (
        <form
          className="form-stack"
          onSubmit={(e) => {
            e.preventDefault();
            if (selected && !mutation.isPending)
              mutation.mutate({ username: selected, role: "member" });
          }}
        >
          <div className="field">
            <label htmlFor="member-search">Search by username</label>
            <input
              id="member-search"
              value={search}
              disabled={mutation.isPending}
              onChange={(e) => {
                setSearch(e.target.value);
                setSelected("");
              }}
              placeholder="Enter at least 2 characters"
            />
          </div>
          <div className="member-results">
            {term.length < 2 ? (
              <p className="muted small">
                Find someone who's already on Circlo.
              </p>
            ) : query.isFetching || search.trim() !== term ? (
              <p role="status">Finding your people…</p>
            ) : query.isError ? (
              <ApiError error={query.error} retry={() => query.refetch()} />
            ) : !query.data?.length ? (
              <p>No matching usernames.</p>
            ) : (
              query.data.map((user) => (
                <label className="member-option" key={user.username}>
                  <input
                    type="radio"
                    name="member"
                    value={user.username}
                    checked={selected === user.username}
                    disabled={mutation.isPending}
                    onChange={() => setSelected(user.username)}
                  />
                  <span>@{user.username}</span>
                </label>
              ))
            )}
          </div>
          <ApiError error={mutation.error} />
          <button
            className="button primary"
            disabled={!selected || mutation.isPending}
          >
            {mutation.isPending ? "Inviting…" : "Send invitation"}
          </button>
        </form>
      )}
    </Modal>
  );
}
