# Personal expenses — implementation notes

## Completed

The existing personal-expense entities, create handler, category reader, configurations, seeds, migration, and database context registrations were reused. The new work adds authenticated CRUD, database-side filtering/sorting/pagination, default settings, month overrides, calculated dashboard/analysis endpoints, and a separate React personal-expense area.

No group-event repository, handler, or data model was changed. Existing group URLs remain available. The authenticated default is now `/home`; valid login deep links are preserved.

## API

All endpoints require authentication. Controllers obtain UserId from NameIdentifier claims; request bodies contain no UserId. Expenses owned by someone else return the same 404 envelope as missing expenses.

| Method | Route | Purpose |
| --- | --- | --- |
| GET | /api/personal-expenses | Filtered, sorted, paginated journal |
| GET | /api/personal-expenses/{id} | One owned expense with category details |
| PUT | /api/personal-expenses/{id} | Update an owned expense |
| DELETE | /api/personal-expenses/{id} | Soft-delete an owned expense |
| GET, PUT | /api/personal-expense-settings | Read defaults / upsert defaults |
| GET, PUT, DELETE | /api/personal-expense-budgets/{year}/{month} | Effective budget / save override / restore default |
| GET | /api/personal-expenses/dashboard?year=2026&month=9 | Monthly totals and chart data |
| GET | /api/personal-expenses/analysis?fromDate=2026-09-01&toDate=2026-09-30 | Deterministic period comparison and breakdown |

Existing POST expense and GET category behavior remains. Hyphenated route aliases were added; existing `/api/PersonalExpenses` and `/api/PersonalExpenseCategories` routes are retained.

List parameters: year, month, categoryId, search, minAmount, maxAmount, paymentMethod, fromDate, toDate, page, pageSize, sortBy, sortDirection. Page size is 1–100 (default 20). Page is 1–1,000,000. Sort fields: ExpenseDate, CreatedAt, Amount, Description, Category. Default is ExpenseDate DESC, CreatedAt DESC, with Id as a deterministic tie-breaker.

## Decisions

- All reporting uses ExpenseDate. Audit timestamps never determine expense month.
- No date filter means the current UTC month. Supplied year/month fill omitted parts from the current UTC date. Explicit date ranges cannot be combined with year/month.
- Input date ranges include both entered dates; repositories convert the upper bound to an exclusive day boundary. Month ranges are [month start, next month start).
- Supported month years are 1–9998 to permit a safe exclusive next-month boundary.
- Analysis supports up to 3,660 days and requires enough date range before the start for the preceding equivalent-length period.
- Missing settings means no configured limit, currency LKR, warning 80%. Reads do not create rows.
- Zero is a real configured budget. Zero spent with zero budget reports 0%; positive spending with a zero budget is Exceeded and PercentageUsed is null, avoiding division by zero.
- RemainingBalance is null if no budget is configured; otherwise it can be negative. OverBudgetAmount is positive when exceeded.
- Current-month average daily spend uses elapsed UTC calendar days; other months use the full number of days in that month.
- Previous-period percentage change is null when previous total is zero; the UI explains why.
- Amounts/limits support two decimal places, matching the existing decimal(18,2) schema. Currency codes are trimmed, uppercased three-letter labels; changing currency does not perform conversion.
- Removed expenses are excluded from all personal queries. Removed budget overrides are revived on subsequent upserts rather than creating rows conflicting with the existing unique index.
- All reads use AsNoTracking; only mutation targets are tracked. Aggregation, filtering, ordering, and paging execute in the database.

## Frontend

- `/home`: personal/group choice cards, no mandatory modal.
- `/personal-expenses`: month overview, budget status, charts with text equivalents, recent expenses, filtered journal, pagination, add/edit modal, delete confirmation.
- `/personal-expenses/analysis`: date ranges, comparison, category/monthly charts, highest day/category.
- `/personal-expenses/settings`: default limit/currency/warning, selected-month override and removal.

Uses the existing API client, JWT handling, React Hook Form, TanStack Query, MUI charts, theme context, and shared feedback/modal components. Mutations invalidate all personal-expense queries. Auth context already clears queries on session changes. Forms retain values on errors and lock duplicate submissions. Dashboard/list filters are intentionally separate and explained in the UI. Responsive table rows become expense cards on narrow screens. Motion respects reduced-motion preferences.

## Files created for this continuation

- `src/CircloApp.Application/Exceptions/NotFoundException.cs`
- `src/CircloApp.Application/Features/PersonalExpenses/ExpenseCrud.cs`
- `src/CircloApp.Application/Features/PersonalExpenses/PersonalBudgets.cs`
- `src/CircloApp.Application/Features/PersonalExpenses/PersonalAnalyticsHandler.cs`
- `src/CircloApp.Application/Features/PersonalExpenses/DTOs/ExpenseFilter.cs`
- `src/CircloApp.Application/Features/PersonalExpenses/DTOs/ExpenseItem.cs`
- `src/CircloApp.Application/Features/PersonalExpenses/DTOs/PersonalAnalytics.cs`
- `src/CircloApp.Application/Interfaces/IPersonalBudgetRepository.cs`
- `src/CircloApp.Application/Interfaces/IPersonalAnalyticsRepository.cs`
- `src/CircloApp.Infrastructure/Repositories/PersonalBudgetRepository.cs`
- `src/CircloApp.Infrastructure/Repositories/PersonalAnalyticsRepository.cs`
- `src/CircloApp.API/Controllers/PersonalExpenseSettingsController.cs`
- `src/CircloApp.API/Controllers/PersonalExpenseBudgetsController.cs`
- `frontend/src/services/personalExpenseService.js`
- `frontend/src/features/personalExpenses/hooks.js`
- `frontend/src/features/personalExpenses/format.js`
- `frontend/src/pages/Home.jsx`
- `frontend/src/pages/personal/PersonalShared.jsx`
- `frontend/src/pages/personal/PersonalExpenses.jsx`
- `frontend/src/pages/personal/ExpenseForm.jsx`
- `frontend/src/pages/personal/PersonalSettings.jsx`
- `frontend/src/pages/personal/PersonalAnalysis.jsx`
- `frontend/src/pages/personal/PersonalCharts.jsx`

## Existing files extended

- `src/CircloApp.Application/Interfaces/IPersonalExpenseRepository.cs`
- `src/CircloApp.Infrastructure/Repositories/PersonalExpenseRepository.cs`
- `src/CircloApp.Infrastructure/DependencyInjection.cs`
- `src/CircloApp.API/Controllers/PersonalExpensesController.cs`
- `src/CircloApp.API/Controllers/PersonalExpenseCategoriesController.cs`
- `src/CircloApp.API/Middlewares/ExceptionMiddleware.cs`
- `frontend/src/routes/AppRoutes.jsx`
- `frontend/src/layouts/MainLayout.jsx`
- `frontend/src/layouts/NavigationBar.jsx`
- `frontend/src/components/common/UI.jsx`
- `frontend/src/utils/loginDestination.js`
- `frontend/src/index.css`

The pre-existing entity/migration/context changes were not recreated or reverted.

## Deployment and verification

No new migration, package, environment variable, or secret is required. Ensure your already-created `20260928065920_AddPersonalExpenseTracking` migration is applied in the deployment database; do not recreate it. Deploy the backend before the frontend.

Commands used:

```powershell
dotnet build src/CircloApp.API --no-restore
npm --prefix frontend run lint
npm --prefix frontend run build
git diff --check
```

Backend build succeeded with pre-existing warnings. Frontend lint and production build succeeded. No unit/integration tests were added or run, as requested. Authenticated live CRUD and mobile browser verification still need a running local application and a signed-in session; compilation is not proof of runtime database behavior.

## Optional follow-up

Custom category management is deferred. The current model supports user-owned categories, but case-insensitive uniqueness and safe deactivation should be verified against the actual database collation before enabling category writes. No AI features were added.
