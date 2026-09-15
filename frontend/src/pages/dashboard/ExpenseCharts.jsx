import { BarChart, PieChart } from "@mui/x-charts";
import { useReducedMotion } from "framer-motion";
import { formatAmount } from "../../utils/format";
export default function ExpenseCharts({ data, kind }) {
  const reduced = useReducedMotion();
  if (kind === "monthly")
    return (
      <BarChart
        height={255}
        dataset={data}
        skipAnimation={Boolean(reduced)}
        colors={["#8b78d2"]}
        xAxis={[{ scaleType: "band", dataKey: "month" }]}
        series={[
          {
            dataKey: "totalAmount",
            label: "Expenses",
            valueFormatter: formatAmount,
          },
        ]}
      />
    );
  return (
    <PieChart
      height={255}
      skipAnimation={Boolean(reduced)}
      colors={["#7560cc", "#72b29c", "#e4bb79", "#9c92b8", "#a3c7cc"]}
      series={[
        {
          innerRadius: 65,
          paddingAngle: 3,
          cornerRadius: 4,
          data: data.map((item) => ({
            id: item.eventId,
            value: item.totalExpenses,
            label: item.eventName,
          })),
          valueFormatter: (item) => formatAmount(item.value),
        },
      ]}
    />
  );
}
