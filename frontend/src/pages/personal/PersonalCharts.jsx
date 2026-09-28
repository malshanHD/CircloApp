import { BarChart, PieChart } from "@mui/x-charts";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import { useMemo } from "react";
import { useTheme } from "../../context/ThemeContext";
import { useReducedMotion } from "framer-motion";
import { money } from "../../features/personalExpenses/format";
export default function PersonalCharts(props) {
  const { theme } = useTheme();
  const chartTheme = useMemo(() => createTheme({ palette: { mode: theme } }), [theme]);
  return <ThemeProvider theme={chartTheme}><Charts {...props} /></ThemeProvider>;
}
function Charts({ categories = [], trend = [], currency, trendTitle = "Daily spending" }) {
  const reduced = useReducedMotion();
  return <div className="chart-grid personal-charts">
    <section className="card chart-card"><h2>Where it went</h2>
      {categories.length ? <><PieChart height={240} hideLegend skipAnimation={Boolean(reduced)} colors={["#8b78d2", "#72b29c", "#e4bb79", "#a3c7cc", "#b892c8"]} series={[{ innerRadius: 65, data: categories.map((c, id) => ({ id, label: c.name, value: c.total })), valueFormatter: v => money(v.value, currency) }]} />
        <ul className="personal-breakdown">{categories.map(c => <li key={c.categoryId || c.name}><span>{c.name}</span><strong>{money(c.total, currency)} <small>({c.percentage}%)</small></strong></li>)}</ul></> : <p className="muted">Add your first expense to see your category breakdown.</p>}
    </section>
    <section className="card chart-card"><h2>{trendTitle}</h2>
      {trend.some(d => d.total > 0) ? <><BarChart height={260} skipAnimation={Boolean(reduced)} colors={["#8b78d2"]} dataset={trend} xAxis={[{ scaleType: "band", dataKey: "label", tickLabelStyle: { fontSize: 10 } }]} series={[{ dataKey: "total", label: "Spending", valueFormatter: v => money(v, currency) }]} />
        <details><summary>View spending values</summary><ul className="personal-breakdown">{trend.map(d => <li key={d.label}><span>{d.label}</span><strong>{money(d.total, currency)}</strong></li>)}</ul></details></> : <p className="muted">No spending in this period. A fresh start.</p>}
    </section>
  </div>;
}
