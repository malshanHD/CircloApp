import { NavLink } from "react-router-dom";
import { ApiError, Skeleton } from "../../components/common/UI";
export function PersonalNav() {
  return <nav className="personal-nav" aria-label="Personal expense navigation">
    <NavLink end to="/personal-expenses">My spending</NavLink>
    <NavLink to="/personal-expenses/analysis">Insights</NavLink>
    <NavLink to="/personal-expenses/settings">Budget settings</NavLink>
  </nav>;
}
export function QueryState({ query, children }) {
  if (query.isPending) return <Skeleton count={2} />;
  if (query.isError) return <ApiError error={query.error} retry={() => query.refetch()} />;
  return children(query.data);
}
export function Stat({ title, children, note }) {
  return <div className="card personal-stat"><span className="muted small">{title}</span><strong>{children}</strong>{note && <p className="small muted">{note}</p>}</div>;
}
