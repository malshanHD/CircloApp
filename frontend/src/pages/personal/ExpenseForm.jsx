import { useRef } from "react";
import { useForm } from "react-hook-form";
import { Modal, Field, ApiError, Skeleton } from "../../components/common/UI";
import { personalExpenseService as service } from "../../services/personalExpenseService";
import { usePersonalQuery, usePersonalMutation } from "../../features/personalExpenses/hooks";
import { localDate, paymentMethods } from "../../features/personalExpenses/format";
export default function ExpenseForm({ expense, onClose, onSaved }) {
  const categories = usePersonalQuery("/personal-expense-categories");
  const lock = useRef(false);
  const { register, handleSubmit, formState: { errors } } = useForm({ defaultValues: {
    amount: expense?.amount ?? "", description: expense?.description ?? "", expenseDate: expense?.expenseDate ?? localDate(),
    categoryId: expense?.categoryId ?? "", paymentMethod: expense?.paymentMethod ?? "", note: expense?.note ?? "",
  } });
  const mutation = usePersonalMutation(data => service.save(data, expense?.id), onSaved);
  async function submit(data) {
    if (lock.current) return;
    lock.current = true;
    try { await mutation.mutateAsync({ ...data, amount: Number(data.amount), categoryId: data.categoryId || null, paymentMethod: data.paymentMethod || null, note: data.note || null }); }
    catch { /* ApiError retains the form and entered values. */ }
    finally { lock.current = false; }
  }
  return <Modal title={expense ? "Edit your expense" : "A little spent. All accounted for."} onClose={onClose} busy={mutation.isPending}>
    <form className="form-stack" onSubmit={event => handleSubmit(submit)(event)} noValidate>
      <fieldset disabled={mutation.isPending} className="form-stack login-fields">
        <div className="personal-amount"><Field label="Amount" type="number" inputMode="decimal" step="0.01" min="0.01" autoFocus registration={register("amount", { required: "Enter an amount.", min: { value: .01, message: "Amount must be positive." }, validate: v => Number.isFinite(Number(v)) && /^\d+(\.\d{1,2})?$/.test(String(v)) || "Use a positive amount with up to two decimal places." })} error={errors.amount} /></div>
        <Field label="Description" maxLength={250} registration={register("description", { required: "What was this expense for?", validate: v => Boolean(v.trim()) || "Enter a description." })} error={errors.description} />
        <Field label="Expense date" type="date" registration={register("expenseDate", { required: "Choose an expense date." })} error={errors.expenseDate} />
        {categories.isPending ? <Skeleton count={1} /> : categories.isError ? <ApiError error={categories.error} retry={() => categories.refetch()} /> : <div className="field"><label htmlFor="personal-category">Category</label><select id="personal-category" {...register("categoryId")}>
          <option value="">Uncategorized</option>{categories.data.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select></div>}
        <div className="field"><label htmlFor="personal-payment">Payment method</label><select id="personal-payment" {...register("paymentMethod")}><option value="">Not specified</option>{expense?.paymentMethod && !paymentMethods.includes(expense.paymentMethod) && <option>{expense.paymentMethod}</option>}{paymentMethods.map(p => <option key={p}>{p}</option>)}</select></div>
        <div className="field"><label htmlFor="personal-note">Note (optional)</label><textarea id="personal-note" rows={3} maxLength={500} {...register("note")} /></div>
        <ApiError error={mutation.error} />
        <button className="button primary wide" disabled={mutation.isPending || categories.isPending || categories.isError}>{mutation.isPending ? "Saving…" : expense ? "Save changes" : "Save expense"}</button>
      </fieldset>
    </form>
  </Modal>;
}
