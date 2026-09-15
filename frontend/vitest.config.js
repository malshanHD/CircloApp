import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",
    setupFiles: ["./tests/setup.js"],
    include: ["tests/**/*.test.{js,jsx}"],
    env: { VITE_API_BASE_URL: "http://127.0.0.1:4179/api" },
    testTimeout: 15000,
  },
});
