import { useForm } from "react-hook-form";
import { useAddExpense } from "../../features/events/hooks";
import { Modal, Field, ApiError } from "../../components/common/UI";
export default function AddExpense({
  eventId,
  onClose,
  onSuccess,
  settlement = false,
}) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();
  const mutation = useAddExpense(eventId, () => {
    onClose();
    onSuccess(
      settlement
        ? "Settlement recorded. Balances have been refreshed."
        : "Expense recorded. Balances have been refreshed.",
    );
  });
  return (
    <Modal
      title={settlement ? "Record a settlement" : "Add an expense"}
      onClose={onClose}
      busy={mutation.isPending}
    >
      <p className="muted small">
        {settlement
          ? "Circlo will allocate the amount using this event's balances."
          : "Record what you paid for this event."}{" "}
        Amounts use the event's agreed currency.
      </p>
      <form
        noValidate
        className="form-stack"
        onSubmit={handleSubmit((data) => {
          if (!mutation.isPending)
            mutation.mutate({
              amount: Number(data.amount),
              description: data.description || "Settlement",
              transactionType: settlement ? 2 : 1,
            });
        })}
      >
        <Field
          label="Amount"
          type="number"
          inputMode="decimal"
          step="0.01"
          registration={register("amount", {
            required: "Enter an amount.",
            validate: (value) =>
              (Number.isFinite(Number(value)) && Number(value) > 0) ||
              "Enter a positive amount.",
          })}
          error={errors.amount}
        />
        {!settlement && (
          <Field
            label="Description"
            placeholder="What was it for?"
            registration={register("description", {
              required: "Describe the expense.",
              validate: (value) =>
                Boolean(value.trim()) || "Describe the expense.",
            })}
            error={errors.description}
          />
        )}
        <ApiError error={mutation.error} />
        <button className="button primary" disabled={mutation.isPending}>
          {mutation.isPending
            ? "Saving…"
            : settlement
              ? "Record settlement"
              : "Add expense"}
        </button>
      </form>
    </Modal>
  );
}
