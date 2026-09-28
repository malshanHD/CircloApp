import { useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { Page, Field, ApiError, Success } from "../../components/common/UI";
import { usePersonalQuery, usePersonalMutation } from "../../features/personalExpenses/hooks";
import { personalExpenseService as service } from "../../services/personalExpenseService";
import { currentMonth, monthParams, money } from "../../features/personalExpenses/format";
import { PersonalNav, QueryState } from "./PersonalShared";
const limitRules = { required: "Enter a limit, including zero if needed.", min: { value: 0, message: "The limit cannot be negative." }, validate: v => /^\d+(\.\d{1,2})?$/.test(String(v)) || "Use up to two decimal places." };
function DefaultForm({ data }) {
  const [saved, setSaved] = useState(false);
  const lock = useRef(false);
  const { register, handleSubmit, formState: { errors } } = useForm({ defaultValues: { ...data, defaultMonthlyLimit: data.defaultMonthlyLimit ?? "" } });
  const mutation = usePersonalMutation(service.settings, () => setSaved(true));
  async function submit(values) {
    if (lock.current) return;
    lock.current = true; setSaved(false);
    try { await mutation.mutateAsync({ ...values, defaultMonthlyLimit: Number(values.defaultMonthlyLimit), warningPercentage: Number(values.warningPercentage), currencyCode: values.currencyCode.trim().toUpperCase() }); }
    catch { /* Preserve editable input on errors. */ } finally { lock.current = false; }
  }
  return <form className="card form-stack personal-settings-form" onSubmit={e => handleSubmit(submit)(e)} noValidate><h2>Your everyday defaults</h2><p className="muted small">The default monthly limit applies to every month unless you set a custom limit. Changing currency changes display labels; it does not convert existing amounts.</p>
    <fieldset disabled={mutation.isPending} className="form-stack login-fields">
      <Field label="Default monthly limit" type="number" step="0.01" min="0" registration={register("defaultMonthlyLimit", limitRules)} error={errors.defaultMonthlyLimit} />
      <Field label="Currency code" maxLength={3} placeholder="LKR" registration={register("currencyCode", { required: "Enter a currency code.", pattern: { value: /^[A-Za-z]{3}$/, message: "Use three letters, such as LKR." } })} error={errors.currencyCode} />
      <Field label="Warn me at this percentage" type="number" min="1" max="100" step="0.01" registration={register("warningPercentage", { required: "Enter a warning percentage.", min: { value: 1, message: "Use 1–100." }, max: { value: 100, message: "Use 1–100." } })} error={errors.warningPercentage} />
      <ApiError error={mutation.error} />{saved && <Success>Defaults saved.</Success>}<button className="button primary" disabled={mutation.isPending}>{mutation.isPending ? "Saving…" : "Save defaults"}</button>
    </fieldset></form>;
}
function OverrideForm({ data, params, currency }) {
  const [message, setMessage] = useState("");
  const lock = useRef(false);
  const { register, handleSubmit, reset, formState: { errors } } = useForm({ defaultValues: { limitAmount: data.limitAmount ?? "" } });
  const save = usePersonalMutation(service.budget, () => setMessage("Custom limit saved for this month."));
  const remove = usePersonalMutation(service.removeBudget, () => { setMessage("Custom limit removed. This month uses your default again."); reset({ limitAmount: "" }); });
  const busy = save.isPending || remove.isPending;
  async function run(values, removing = false) {
    if (lock.current) return;
    lock.current = true; setMessage(""); save.reset(); remove.reset();
    try { await (removing ? remove.mutateAsync(params) : save.mutateAsync({ ...params, limitAmount: Number(values.limitAmount) })); }
    catch { /* Errors stay alongside the selected month. */ } finally { lock.current = false; }
  }
  return <form className="card form-stack personal-settings-form" onSubmit={e => handleSubmit(v => run(v))(e)} noValidate><h2>Just for this month</h2><p className="muted small">This custom limit applies only to the selected month. Removing it restores your default.</p><p>Currently: <strong>{data.effectiveMonthlyLimit == null ? "No monthly limit set" : money(data.effectiveMonthlyLimit, currency)}</strong> · {data.limitSource === "MonthlyOverride" ? "Custom limit" : "Default"}</p>
    <Field label="Custom monthly limit" type="number" step="0.01" min="0" disabled={busy} registration={register("limitAmount", limitRules)} error={errors.limitAmount} />
    <ApiError error={save.error || remove.error} />{message && <Success>{message}</Success>}
    <button className="button primary" disabled={busy}>{save.isPending ? "Saving…" : "Save custom limit"}</button>
    {data.limitSource === "MonthlyOverride" && <button type="button" className="button secondary" disabled={busy} onClick={() => run(null, true)}>{remove.isPending ? "Removing…" : "Remove custom limit"}</button>}
  </form>;
}
export default function PersonalSettings() {
  const [search] = useSearchParams();
  const requested = search.get("month");
  const [month, setMonth] = useState(/^\d{4}-(0[1-9]|1[0-2])$/.test(requested || "") ? requested : currentMonth());
  const params = monthParams(month);
  const settings = usePersonalQuery("/personal-expense-settings");
  const budget = usePersonalQuery(`/personal-expense-budgets/${params.year}/${params.month}`);
  return <Page className="personal-page"><div className="page-heading"><div><span className="eyebrow">ROOM FOR WHAT MATTERS</span><h1>Budget settings</h1><p>A gentle guide for your monthly spending.</p></div></div><PersonalNav />
    <label className="personal-month">Month for custom limit<input type="month" value={month} min="0001-01" max="9998-12" onChange={e => { if (e.target.value) setMonth(e.target.value); }} /></label>
    <div className="chart-grid"><QueryState query={settings}>{d => <DefaultForm data={d} />}</QueryState><QueryState query={budget}>{d => <OverrideForm key={month} data={d} params={params} currency={settings.data?.currencyCode || "LKR"} />}</QueryState></div>
  </Page>;
}
