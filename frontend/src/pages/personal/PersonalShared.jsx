import { ApiError, Skeleton } from "../../components/common/UI";
export function QueryState({ query, children }) {
  if (query.isPending) return <Skeleton count={2} />;
  if (query.isError) return <ApiError error={query.error} retry={() => query.refetch()} />;
  return children(query.data);
}
export function Stat({ title, children, note }) {
  return <div className="card personal-stat"><span className="muted small">{title}</span><strong>{children}</strong>{note && <p className="small muted">{note}</p>}</div>;
}
