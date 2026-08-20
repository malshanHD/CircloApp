import { BarChart, PieChart } from "@mui/x-charts";
import Navigation from "../../layouts/NavigationBar";
import { useQuery } from "@tanstack/react-query";
import { expensesService } from "../../services/expenseService";
import loadingGif from "../../assets/loading.gif";
import { useState } from "react";

const Dashboard = () => {
  const [year, setYear] = useState(new Date().getFullYear());

  const { data, isLoading, isError } = useQuery({
    queryKey: ["my-expenses"],
    queryFn: async () => {
      const response = await expensesService.get("/Expenses");
      return response;
    },
  });

  const chartData =
    data?.map((item) => ({
      id: item.eventId,
      value: item.totalExpenses,
      label: item.eventName,
    })) || [];

  const {
    data: barchartData = [],
    isLoading: barchartLoading,
    isError: barchartError,
  } = useQuery({
    queryKey: ["my-monthly-expenses", year],
    queryFn: async () => {
      const barChartResponse = await expensesService.get(`/Expenses/${year}`);
      return barChartResponse;
    },
  });

  return (
    <div>
      <Navigation />

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 w-full p-4">
        <div className="w-full bg-white p-4 rounded-xl shadow-md border border-gray-100 h-[300px] flex justify-center items-center">
          {!isLoading ? (
            <PieChart
              series={[
                {
                  data: chartData,
                },
              ]}
              width={200}
              height={200}
            />
          ) : (
            <img
              src={loadingGif}
              alt="Loading..."
              className="w-16 h-16 object-contain"
            />
          )}
        </div>

        <div className="w-full bg-white p-4 rounded-xl shadow-md border border-gray-100 h-[300px] flex justify-center items-center">
          {!barchartLoading ? (
            <BarChart
              dataset={barchartData}
              xAxis={[{ scaleType: "band", dataKey: "month" }]}
              yAxis={[{ label: "Total Amount (LKR)" }]}
              series={[
                {
                  dataKey: "totalAmount",
                  label: "Expenses",
                  valueFormatter: (value) => `$${value?.toLocaleString() ?? 0}`,
                },
              ]}
            />
          ) : (
            <img
              src={loadingGif}
              alt="Loading..."
              className="w-16 h-16 object-contain"
            />
          )}
        </div>

        <div className="w-full bg-white p-4 rounded-xl shadow-md border border-gray-100 h-[300px] flex justify-center items-center">
          <PieChart
            series={[
              {
                data: [
                  { id: 0, value: 10, label: "series A" },
                  { id: 1, value: 15, label: "series B" },
                  { id: 2, value: 20, label: "series C" },
                ],
              },
            ]}
            width={200}
            height={200}
          />
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
