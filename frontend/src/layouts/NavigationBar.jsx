import { Link, useNavigate } from "react-router-dom";
import { auth } from "../utils/auth";
import { useState } from "react";
import CreateEventModal from "../pages/events/CreateEventModal";

const Navigation = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    auth.logout();
    navigate("/login", { replace: true });
  };

  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const toggleMenu = () => {
    setIsMenuOpen((prev) => !prev);
  };

  const [isModalOpen, setIsModalOpen] = useState(false);

  return (
    <div className="bg-white p-8 flex justify-center items-start">
      <nav className="relative flex items-center justify-between gap-8 border border-slate-800/80 bg-black/90 backdrop-blur-md px-6 py-3 rounded-full text-white text-sm w-fit shadow-2xl">
        {/* Logo */}
        <a href="https://prebuiltui.com" className="flex items-center">
          <svg
            width="28"
            height="28"
            viewBox="0 0 32 32"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <circle cx="4.706" cy="16" r="4.706" fill="#D9D9D9" />
            <circle cx="16.001" cy="4.706" r="4.706" fill="#D9D9D9" />
            <circle cx="16.001" cy="27.294" r="4.706" fill="#D9D9D9" />
            <circle cx="27.294" cy="16" r="4.706" fill="#D9D9D9" />
          </svg>
        </a>

        {/* Navigation Links */}
        <div className="hidden md:flex items-center gap-8 font-medium text-white">
          <Link
            className="hover:text-slate-300 transition-colors"
            to={"/dashboard"}
          >
            Dashboard
          </Link>
          <Link
            className="hover:text-slate-300 transition-colors"
            to={"/events"}
          >
            My Events
          </Link>
          <button
            onClick={handleLogout}
            className="border border-slate-700/80 hover:bg-slate-800/60 px-5 py-2 rounded-full text-sm font-medium transition text-white"
          >
            Logout
          </button>
        </div>

        {/* Buttons */}
        <div className="hidden md:flex items-center gap-3">
          <button className="border border-slate-700/80 hover:bg-slate-800/60 px-5 py-2 rounded-full text-sm font-medium transition text-white">
            Contact
          </button>

          {/* Glowing "Get Started" Button */}
          <button
            onClick={() => setIsModalOpen(true)}
            className="bg-white text-black font-semibold px-5 py-2 rounded-full text-sm shadow-[0_0_25px_rgba(255,255,255,0.6)] hover:shadow-[0_0_35px_rgba(255,255,255,0.8)] transition-all duration-300"
          >
            Create a Event
          </button>

          {/* Render Modal */}
          <CreateEventModal
            isOpen={isModalOpen}
            onClose={() => setIsModalOpen(false)}
          />
        </div>

        {/* Mobile Hamburger Toggle */}
        <button
          onClick={() => setIsMenuOpen(!isMenuOpen)}
          className="md:hidden text-gray-300 hover:text-white"
        >
          <svg
            className="w-6 h-6"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            viewBox="0 0 24 24"
          >
            <path
              d="M4 6h16M4 12h16M4 18h16"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        </button>

        {/* Mobile Dropdown */}
        {isMenuOpen && (
          <div className="absolute top-16 left-0 w-full bg-black/95 border border-slate-800 p-6 rounded-2xl flex flex-col items-center gap-4 md:hidden z-50">
            <Link className="hover:text-slate-300" to={"/dashboard"}>
              Dashboard
            </Link>
            <Link className="hover:text-slate-300" to={"/events"}>
              My Events
            </Link>
            <button className="w-full border border-slate-700 py-2 rounded-full text-white">
              Contact
            </button>
            <button className="w-full bg-white text-black py-2 rounded-full font-semibold shadow-[0_0_20px_rgba(255,255,255,0.5)]">
              Get Started
            </button>
            <button
              onClick={() => setIsModalOpen(true)}
              className="w-full bg-white text-black py-2 rounded-full font-semibold shadow-[0_0_20px_rgba(255,255,255,0.5)]"
            >
              Create a Event
            </button>
          </div>
        )}
      </nav>
    </div>
  );
};

export default Navigation;
