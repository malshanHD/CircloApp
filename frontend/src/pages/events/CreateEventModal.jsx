import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { useCreateEvent, isEventId } from "../../features/events/hooks";
import { Modal, Field, ApiError } from "../../components/common/UI";
export default function CreateEventModal({ isOpen, onClose }) {
  const navigate = useNavigate();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();
  const mutation = useCreateEvent((result) => {
    onClose();
    navigate(
      isEventId(result.eventId) ? `/events/${result.eventId}` : "/events",
      { state: { success: result.message || "Your event is ready." } },
    );
  });
  if (!isOpen) return null;
  return (
    <Modal
      title="Start something together."
      onClose={onClose}
      busy={mutation.isPending}
    >
      <p className="muted small">
        A weekend away, a shared dinner, or your next big idea.
      </p>
      <form
        noValidate
        className="form-stack"
        onSubmit={handleSubmit((data) => {
          if (!mutation.isPending) mutation.mutate(data);
        })}
      >
        <Field
          label="Event name"
          placeholder="e.g. Our weekend escape"
          registration={register("name", {
            required: "Give your event a name.",
            maxLength: { value: 100, message: "Use 100 characters or fewer." },
            validate: (value) =>
              Boolean(value.trim()) || "Enter an event name.",
          })}
          error={errors.name}
        />
        <div className="field">
          <label htmlFor="event-description">Description</label>
          <textarea
            id="event-description"
            rows={4}
            placeholder="What's the plan?"
            aria-invalid={Boolean(errors.description)}
            aria-describedby={
              errors.description ? "description-error" : undefined
            }
            {...register("description", {
              required: "Add a description.",
              maxLength: {
                value: 500,
                message: "Use 500 characters or fewer.",
              },
              validate: (value) =>
                Boolean(value.trim()) || "Add a description.",
            })}
          />
          {errors.description && (
            <p id="description-error" className="field-error">
              {errors.description.message}
            </p>
          )}
        </div>
        <ApiError error={mutation.error} />
        <div className="form-actions">
          <button
            type="button"
            className="button secondary"
            onClick={onClose}
            disabled={mutation.isPending}
          >
            Cancel
          </button>
          <button className="button primary" disabled={mutation.isPending}>
            {mutation.isPending ? "Creating…" : "Create event"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
