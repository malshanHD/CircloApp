import React, { useState, useEffect } from "react";
import { createPortal } from "react-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { FiX, FiSearch, FiCheck, FiUser } from "react-icons/fi";
import loadingGif from "../../assets/loading.gif";
import { userService } from "../../services/userService"; // Adjust path
import { eventService } from "../../services/eventService"; // Adjust path

const AddMemberModal = ({ isOpen, onClose, eventName, eventId }) => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");

  // Selected user is now stored directly as a username string (e.g. 'mahela27')
  const [selectedUser, setSelectedUser] = useState(null);

  // 1. Debounce search input
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearch(searchTerm);
    }, 300);
    return () => clearTimeout(handler);
  }, [searchTerm]);

  // 2. Query to search usernames from backend (e.g., returns ['mahela27', 'malshanhans'])
  const { data: users = [], isFetching, isError: fetchingError, error: fetchingErrorMessage } = useQuery({
    queryKey: ["searchUsers", debouncedSearch],
    queryFn: () => userService.searchUsers(debouncedSearch),
    enabled: debouncedSearch.trim().length >= 2,
  });

  // 3. Mutation to add username to event
  const {
    mutate: addMember,
    isPending,
    isError,
    error,
  } = useMutation({
    mutationFn: (username) => eventService.addMemberToEvent(eventId, username),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
      handleClose();
    },
  });

  const handleClose = () => {
    setSearchTerm("");
    setDebouncedSearch("");
    setSelectedUser(null);
    onClose();
  };

  const handleAddUser = () => {
    if (selectedUser) {
      const payload = {
        username: selectedUser,
        role: "User",
      };
      addMember(payload);
    }
  };

  if (!isOpen) return null;

  const errorMessage =
    error?.response?.data?.message ||
    error?.message ||
    "Failed to create event. Please try again.";

  return createPortal(
    <div className="fixed inset-0 top-0 left-0 w-screen h-screen z-[99999] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div
        className="relative z-[100000] isolate w-full max-w-md bg-white rounded-2xl shadow-2xl overflow-hidden animate-in fade-in zoom-in duration-200"
        style={{ backgroundColor: "#ffffff" }}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-gray-100 bg-white">
          <div>
            <h3 className="text-xl font-bold text-gray-800">Add Member</h3>
            <p className="text-xs text-gray-500 mt-0.5">
              Event:{" "}
              <span className="font-semibold text-blue-600">{eventName}</span>
            </p>
          </div>
          <button
            onClick={handleClose}
            className="text-gray-400 hover:text-gray-600 p-1 rounded-full hover:bg-gray-100 transition-colors"
          >
            <FiX className="w-6 h-6" />
          </button>
        </div>

        {/* Search & Selection Body */}
        <div className="p-6 space-y-4 bg-white">
          {/* Search Input */}
          <div className="relative">
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
              <FiSearch className="w-5 h-5" />
            </span>
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
              placeholder="Type username or email..."
            />
          </div>

          {/* User Results List */}
          <div className="min-h-[160px] max-h-[220px] overflow-y-auto space-y-2 pr-1 border border-gray-100 rounded-xl p-2">
            {isFetching ? (
              <div className="flex items-center justify-center h-32">
                <p className="text-xs text-gray-400">Searching users...</p>
              </div>
            ) : debouncedSearch.trim().length < 2 ? (
              <div className="flex items-center justify-center h-32">
                <p className="text-xs text-gray-400">
                  Enter at least 2 characters to search
                </p>
              </div>
            ) : users.length === 0 ? (
              <div className="flex items-center justify-center h-32">
                <p className="text-xs text-gray-400">
                  No users found matching "{debouncedSearch}"
                </p>
              </div>
            ) : (
              users.map((usernameItem) => {
                // If usernameItem is string 'mahela27' or object { username: 'mahela27' }
                const username =
                  typeof usernameItem === "string"
                    ? usernameItem
                    : usernameItem.username;
                const isSelected = selectedUser === username;

                return (
                  <div
                    key={username}
                    onClick={() =>
                      setSelectedUser(isSelected ? null : username)
                    }
                    className={`flex items-center justify-between p-3 rounded-xl border cursor-pointer transition-all ${
                      isSelected
                        ? "border-blue-500 bg-blue-50/60 shadow-sm"
                        : "border-gray-100 hover:bg-gray-50"
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      <div className="w-9 h-9 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-semibold text-sm">
                        <FiUser className="w-4 h-4" />
                      </div>
                      <p className="text-sm font-semibold text-gray-700">
                        @{username}
                      </p>
                    </div>

                    <div
                      className={`w-6 h-6 rounded-full flex items-center justify-center border transition-all ${
                        isSelected
                          ? "bg-blue-600 border-blue-600 text-white"
                          : "border-gray-300"
                      }`}
                    >
                      {isSelected && <FiCheck className="w-3.5 h-3.5" />}
                    </div>
                  </div>
                );
              })
            )}
          </div>

          {isError && (
            <p className="text-center text-sm font-medium text-red-600">
              {errorMessage}
            </p>
          )}

          {/* Action Buttons */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={handleClose}
              className="w-1/2 py-3 border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold rounded-full text-sm transition"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={handleAddUser}
              disabled={!selectedUser || isPending}
              className="w-1/2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-semibold py-3 rounded-full text-sm transition shadow-md flex items-center justify-center h-11"
            >
              {isPending ? (
                <img
                  src={loadingGif}
                  alt="Adding..."
                  className="w-5 h-5 object-contain"
                />
              ) : (
                "Add User"
              )}
            </button>
          </div>
        </div>
      </div>
    </div>,
    document.body,
  );
};

export default AddMemberModal;
