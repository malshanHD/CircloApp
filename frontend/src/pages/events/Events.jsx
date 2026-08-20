import React from "react";
import { useQuery } from "@tanstack/react-query";
import { FiCalendar, FiUsers, FiArrowRight } from "react-icons/fi";
import { eventService } from "../../services/eventService";
import Navigation from "../../layouts/NavigationBar";

const Events = () => {
  const { data, isLoading, isError } = useQuery({
    queryKey: ["my-events"],
    queryFn: eventService.getMyEvents,
  });

  const events = data?.items ?? [];

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-gray-600">Loading your events...</div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-red-600">Failed to load events.</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <Navigation />

      {/* Main */}
      <main className="max-w-7xl mx-auto px-6 py-10">
        {/* Welcome */}
        <div className="mb-10">
          <h2 className="text-3xl font-bold text-gray-900">Welcome back 👋</h2>

          <p className="mt-2 text-gray-500">
            Here are the events you're participating in.
          </p>
        </div>

        {/* Events */}
        <div>
          <div className="flex items-center justify-between mb-6">
            <div>
              <h3 className="text-xl font-bold text-gray-900">Your Events</h3>

              <p className="text-sm text-gray-500 mt-1">
                {events.length} event{events.length !== 1 ? "s" : ""}
              </p>
            </div>
          </div>

          {events.length === 0 ? (
            <div className="bg-white rounded-2xl border border-dashed border-gray-300 p-12 text-center">
              <FiCalendar className="w-12 h-12 mx-auto text-gray-400" />

              <h3 className="mt-4 text-lg font-semibold text-gray-800">
                No events yet
              </h3>

              <p className="mt-2 text-gray-500">
                You haven't been enrolled in any events yet.
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {events.map((event) => (
                <div
                  key={event.id}
                  className="bg-white rounded-2xl border border-gray-200 p-6 hover:shadow-lg transition-all duration-200"
                >
                  {/* Event Icon */}
                  <div className="w-12 h-12 rounded-xl bg-blue-50 flex items-center justify-center mb-5">
                    <FiCalendar className="w-6 h-6 text-blue-600" />
                  </div>

                  {/* Event Name */}
                  <h3 className="text-xl font-bold text-gray-900">
                    {event.name}
                  </h3>

                  {/* Description */}
                  {event.description && (
                    <p className="text-gray-500 text-sm mt-2 line-clamp-2">
                      {event.description}
                    </p>
                  )}

                  {/* Event Information */}
                  <div className="mt-6 space-y-3">
                    <div className="flex items-center gap-3 text-gray-600">
                      <FiUsers className="w-5 h-5 text-gray-400" />

                      <span className="text-sm">
                        {event.memberCount ?? 0} members
                      </span>
                    </div>
                  </div>

                  {/* View Event */}
                  <button className="mt-6 w-full flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 rounded-xl transition">
                    View Event
                    <FiArrowRight className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
};

export default Events;
