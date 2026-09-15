# Circlo frontend

React + Vite, JavaScript/JSX. See INTEGRATION.md for the verified endpoint inventory, setup, test results, backend gaps, and isolated browser fixtures.

## Start

```powershell
npm ci
npm run dev
```

Set VITE_API_BASE_URL in .env to the API base including /api; see .env.example. Use http://localhost:5173 with the current backend CORS policy.

## Verify

```powershell
npm test
npm run lint
npm run build
```

The backend must be available for real registration, event, and AI requests. Synthetic fixtures are confined to tests and are never imported by production components.
