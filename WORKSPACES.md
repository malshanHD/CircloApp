# Circlo workspace redesign

The authenticated shell now has Personal and Groups workspaces. Existing API contracts, event operations, personal expense operations, authentication and query ownership are retained.

## Navigation behavior

- Groups is the default when no valid preference is saved. `/home` is now a compatibility landing redirect to the remembered workspace rather than a mandatory selection screen.
- `circlo-workspace-mode` remembers the selected workspace. `circlo-workspace-last-route-personal` and `circlo-workspace-last-route-groups` remember destinations including query strings and fragments.
- Direct routes and browser back/forward determine the active mode. Switching uses React Router without reloading or clearing query caches.
- Stored destinations must resolve to a known route in the selected workspace. External URLs, protocol-relative URLs, backslashes, whitespace and unknown routes fall back to that workspace's home.
- Invitation routes select Groups but are not remembered: reopening them can submit a join request. Existing invitation behavior is unchanged.
- Unavailable local storage is handled without breaking navigation.

## UI

- Central configuration controls workspace icons, navigation, actions, home routes and supporting copy.
- Personal exposes overview/expenses, insights and budget settings. Groups exposes overview, events and event-scoped Circlo AI. Join notifications mount only in Groups.
- A quiet sidebar shortcut opens the existing expense form or event form. Personal contextual actions reuse that same expense form. Existing editing and deletion stay on the personal page.
- Mobile has an always-visible mode switcher, a dismissible navigation drawer and a reachable floating primary action. Redundant page creation buttons are hidden at mobile widths; event invitation controls remain available.
- Feather icons, pressed-state semantics, focus outlines, 44px workspace controls, native dialog focus handling, reduced motion and dark colors are retained or improved.
- The shell stays mounted while lazy destination screens load. The remaining balance card and existing financial calculations are retained.
- Authentication storage events now react only to session changes, so preferences in another tab do not unnecessarily clear server data.

## Files

Created:
- frontend/src/features/workspace/config.js
- frontend/src/features/workspace/WorkspaceContext.jsx
- frontend/src/features/workspace/WorkspaceSwitcher.jsx

Updated:
- frontend/src/layouts/MainLayout.jsx
- frontend/src/layouts/NavigationBar.jsx
- frontend/src/routes/AppRoutes.jsx
- frontend/src/context/AuthContext.jsx
- frontend/src/index.css
- frontend/src/pages/personal/PersonalExpenses.jsx
- frontend/src/pages/personal/PersonalAnalysis.jsx
- frontend/src/pages/personal/PersonalSettings.jsx
- frontend/src/pages/personal/PersonalShared.jsx
- frontend/src/pages/events/Events.jsx
- frontend/src/pages/dashboard/Dashboard.jsx

## Validation

Commands: `npm --prefix frontend run lint`, `npm --prefix frontend run build`, `git diff --check`.
No unit tests were added or run. No backend changes, new dependencies or migrations.
Responsive CSS was reviewed for 375px, 768px, 1280px and larger layouts. Authenticated browser screenshots and interactive checks at those widths still need verification; no signed-in browser session was available. Check switching, refresh, back/forward, direct event links, both themes and the add-expense drawer/dialog flow during acceptance review.

Optional later: a dismissible workspace discovery hint. Personal AI remains out of scope.

The signed-in registration redirect also uses /home so it restores the workspace instead of forcing Groups.
