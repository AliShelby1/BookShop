# BookShop — Full ASP.NET Core MVC Learning & Development Plan

## 1. Project Overview

**Project name:** BookShop  
**Framework:** ASP.NET Core MVC on **.NET 8**  
**Database:** SQL Server  
**ORM:** Entity Framework Core 8  
**Authentication:** ASP.NET Core Identity  
**Authorization:** Role-based initially, with policy-based authorization introduced where useful  
**Frontend:** Razor Views + Bootstrap + custom CSS + JavaScript  
**Testing:** xUnit + ASP.NET Core integration testing  
**Version control:** Git

BookShop is both:

1. A fully functional online bookstore.
2. A long-term learning project designed to build strong practical understanding of modern .NET backend/MVC development.

The goal is **not** to create a simple CRUD tutorial application. The final project should demonstrate production-oriented architecture, clean code, real business rules, security, testing, performance awareness, good UI/UX, and professional development practices.

---

# 2. Current State

The user has already:

- Created the BookShop solution.
- Created the ASP.NET Core MVC application.
- Configured authentication.
- Configured role-based authorization.
- Is using **.NET 8**.

## Important constraint

Do **not** migrate the project to .NET 10.

The existing application is intentionally based on .NET 8. Keep the project on .NET 8 unless the user explicitly asks for a framework upgrade.

Before modifying the project:

- Inspect the existing solution.
- Inspect the existing `.csproj`.
- Inspect `Program.cs`.
- Inspect Identity configuration.
- Inspect existing Identity/ApplicationDbContext code.
- Inspect existing authentication and authorization setup.
- Preserve working functionality.
- Do not recreate existing authentication/authorization unnecessarily.

---

# 3. Main Learning Objective

The project should teach the user how a professional ASP.NET Core developer thinks and builds applications.

The user should understand:

- C# in a real application.
- ASP.NET Core request pipeline.
- MVC.
- Dependency Injection.
- Configuration.
- Authentication.
- Authorization.
- Entity Framework Core.
- SQL/database design.
- LINQ.
- Async programming.
- Business logic.
- Service layer design.
- DTOs and ViewModels.
- Validation.
- Error handling.
- Logging.
- Security.
- Testing.
- API development.
- Performance.
- Caching.
- Background processing.
- File handling.
- Git.
- Deployment.
- Refactoring and architectural decisions.

Do not optimize the project around simply finishing features. Each feature should also teach the underlying concept.

---

# 4. Development Philosophy

## 4.1 Build in vertical slices

Features should be implemented end-to-end.

For example:

```text
Requirement
    ↓
Domain/database design
    ↓
Entity
    ↓
EF Core configuration
    ↓
Migration
    ↓
Application/business logic
    ↓
ViewModel/DTO
    ↓
Controller
    ↓
View
    ↓
Validation
    ↓
Authorization
    ↓
Error handling
    ↓
Testing
    ↓
UI/UX polish
    ↓
Refactoring
```

Do not create dozens of unrelated entities/controllers first and attempt to connect them later.

---

## 4.2 Teach before abstracting

Do not introduce unnecessary abstractions simply because they appear in tutorials.

Examples:

- Do not automatically create a generic repository for every EF Core operation.
- Do not introduce design patterns without a concrete reason.
- Do not create excessive interfaces.
- Do not add layers that provide no practical benefit.

The user should understand:

> Why does this abstraction exist?

rather than:

> Every .NET application must have this abstraction.

---

## 4.3 Prefer real business logic over artificial CRUD

BookShop should contain meaningful rules.

Examples:

- A customer cannot order more than available stock.
- A customer cannot review a book unless the chosen business rule allows it.
- A coupon cannot be used after expiration.
- Invalid order status transitions should be rejected.
- Historical order prices must not change when current book prices change.
- Customers can only access their own orders.
- Admin functionality must be protected.
- Deleted/deactivated books should be handled correctly in existing orders.

---

## 4.4 Explain important decisions

When implementing significant functionality, explain:

- What is being built.
- Why it is designed this way.
- What alternatives exist.
- Why the chosen design is appropriate.
- What trade-offs exist.

Avoid excessive theory disconnected from the current implementation.

---

# 5. Target Architecture

Use a layered architecture:

```text
BookShop
│
├── BookShop.Web
│   ├── Controllers
│   ├── Areas
│   ├── Views
│   ├── ViewModels
│   ├── Filters
│   ├── wwwroot
│   └── Program.cs
│
├── BookShop.Application
│   ├── Interfaces
│   ├── Services
│   ├── DTOs
│   ├── ViewModels
│   ├── Validators
│   ├── Exceptions
│   └── Common
│
├── BookShop.Domain
│   ├── Entities
│   ├── Enums
│   ├── ValueObjects
│   └── Common
│
├── BookShop.Infrastructure
│   ├── Data
│   ├── Configurations
│   ├── Repositories (only where justified)
│   ├── Services
│   ├── Storage
│   └── Migrations
│
└── BookShop.Tests
    ├── Unit
    └── Integration
```

## Dependency direction

```text
Web
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application + Domain
```

The domain should not depend on the web layer.

The business/application layer should not depend directly on Razor views.

Infrastructure concerns such as EF Core, file storage, email and external services should remain outside the domain.

---

# 6. Core Entities

The exact model can evolve during implementation, but the planned core entities are:

```text
ApplicationUser
Book
Author
Category
Publisher

Cart
CartItem

Wishlist
WishlistItem

Address

Order
OrderItem

Review

Coupon
CouponUsage

InventoryTransaction
```

Potential relationship/junction entities:

```text
BookAuthor
BookCategory
```

Use junction entities only where they provide real value; do not create redundant relationships.

---

# 7. Important Domain Decisions

## 7.1 Book

Expected fields may include:

```text
Id
ISBN
Title
Description
Price
DiscountPercentage or appropriate pricing representation
StockQuantity
CoverImageUrl
PublisherId
PublishedDate
IsActive
CreatedAt
UpdatedAt
```

Exact fields should be reviewed before implementation.

---

## 7.2 Order history

Order items should preserve historical purchase information.

For example:

```text
OrderItem
---------------
Id
OrderId
BookId
BookTitle
UnitPrice
Quantity
Subtotal
```

Do not calculate historical order totals from the current `Book.Price`.

A book changing from 20 to 30 later must not change an old order from 20 to 30.

---

## 7.3 Order status

Create an enum or equivalent domain representation.

Potential statuses:

```text
Pending
Confirmed
Preparing
Shipped
Delivered
Cancelled
```

Implement explicit transition rules instead of allowing arbitrary status changes.

---

## 7.4 Payment status

Potential states:

```text
Pending
Paid
Failed
Refunded
```

Payment integration can remain simulated/local unless the user explicitly requests a real provider.

---

# 8. Project Phases

## Phase 0 — Inspect and stabilize the current solution

### Goals

- Understand existing solution.
- Preserve existing Identity/authentication/authorization.
- Establish baseline architecture.
- Verify the project builds and runs.

### Tasks

1. Inspect solution and project files.
2. Confirm .NET 8.
3. Confirm packages and package versions.
4. Inspect `Program.cs`.
5. Inspect Identity setup.
6. Inspect database context.
7. Inspect existing roles and seeding.
8. Confirm SQL Server connection configuration.
9. Initialize/verify Git.
10. Create an initial clean commit if appropriate.

### Learning

- Solution vs project.
- SDK-style projects.
- Dependency Injection.
- Middleware.
- Configuration.
- ASP.NET Core startup.

---

# Phase 1 — Domain and database foundation

### Goals

Create the core domain model and database foundation before building complex UI.

### Tasks

- Create Domain project.
- Create Application project.
- Create Infrastructure project.
- Establish project references.
- Create initial domain entities.
- Create enums.
- Configure EF Core.
- Configure relationships.
- Configure constraints.
- Configure indexes where useful.
- Create migrations.
- Create/update SQL Server database.
- Add development seed data.

### Learn

- Domain modeling.
- EF Core.
- DbContext.
- DbSet.
- Migrations.
- Relationships.
- Foreign keys.
- Constraints.
- Indexes.
- Entity configuration.
- `IEntityTypeConfiguration<T>`.

---

# Phase 2 — Admin Book Management

Build the first complete vertical feature.

## Features

- Book list.
- Book details.
- Create book.
- Edit book.
- Activate/deactivate book.
- Delete only where appropriate.
- Search.
- Sorting.
- Pagination.
- Stock display.
- Cover image upload.

## Also build

- Author management.
- Category management.
- Publisher management.

## Architecture

```text
Admin UI
  ↓
Admin Controller
  ↓
Application Service
  ↓
Domain
  ↓
Infrastructure / EF Core
  ↓
SQL Server
```

## Learn

- MVC controllers.
- Actions.
- Model binding.
- Razor.
- ViewModels.
- Validation.
- Tag Helpers.
- Dependency Injection.
- File uploads.
- Pagination.
- LINQ.

---

# Phase 3 — Customer storefront

Build the customer-facing bookstore.

## Home page

- Hero section.
- Featured books.
- New arrivals.
- Popular books.
- Categories.
- Authors.

## Shop page

Support:

- Search.
- Category filtering.
- Author filtering.
- Publisher filtering.
- Price filtering.
- Availability filtering.
- Sorting.
- Pagination.

## Book details

Display:

- Cover.
- Title.
- Author.
- Publisher.
- ISBN.
- Description.
- Price.
- Discount.
- Stock.
- Rating.
- Reviews.
- Related books.

Actions:

```text
Add to Cart
Add to Wishlist
```

## Learn

- ViewModels.
- Projections.
- Query design.
- Razor partials.
- View Components where useful.
- Routing.
- Query strings.
- UI composition.

---

# Phase 4 — Shopping Cart

## Features

- Add to cart.
- Remove from cart.
- Increase quantity.
- Decrease quantity.
- Clear cart.
- Validate stock.
- Calculate subtotal.
- Calculate discounts.
- Calculate total.

## Important rule

Cart logic should not be duplicated in controllers.

Use a dedicated application/service layer.

Example:

```text
CartController
    ↓
ICartService
    ↓
Cart business rules
    ↓
Persistence
```

## Learn

- Services.
- Interfaces.
- Business logic.
- Dependency Injection.
- Async methods.
- State management.

---

# Phase 5 — Wishlist

## Features

- Add item.
- Remove item.
- Move to cart.
- Prevent duplicates.
- Display wishlist count.

## Learn

- Many-to-many/relationship modeling.
- Business rules.
- AJAX/fetch where it improves UX.

---

# Phase 6 — Checkout

## Flow

```text
Cart
 ↓
Checkout
 ↓
Address
 ↓
Order review
 ↓
Place order
 ↓
Order confirmation
```

## Requirements

- Address selection/creation.
- Shipping information.
- Stock validation.
- Price calculation.
- Discount calculation.
- Order creation.
- Stock deduction.
- Order status initialization.
- Payment status initialization.
- Cart clearing.

## Critical behavior

Use a transaction where required so related changes do not leave the system in an inconsistent state.

Example:

```text
Create Order
 ↓
Create OrderItems
 ↓
Update Inventory
 ↓
Clear Cart
 ↓
Commit
```

If a critical step fails, the transaction should be rolled back where appropriate.

---

# Phase 7 — Order management

## Customer

- My Orders.
- Order details.
- Order history.
- Order status.
- Order totals.

Customers must only be able to access their own orders.

## Admin

- Orders list.
- Search.
- Filter by status.
- View details.
- Change status.
- Review payment status.

## Learn

- Authorization.
- Ownership checks.
- Business rules.
- Query projections.
- Status/state transitions.

---

# Phase 8 — Inventory

## Features

- Current stock.
- Stock adjustments.
- Low-stock alert.
- Inventory history.

Potential entity:

```text
InventoryTransaction
--------------------
Id
BookId
Quantity
Type
Reason
CreatedAt
CreatedBy
```

Potential transaction types:

```text
Purchase
Sale
Adjustment
Return
```

Do not unnecessarily duplicate stock state if the domain design does not require it.

---

# Phase 9 — Reviews and ratings

## Customer

- Submit review.
- Rating from 1–5.
- Edit review.
- Delete review.

## Admin

- Review moderation.
- Hide review.
- Delete review if needed.

## Business rule

Decide and implement whether reviews are restricted to customers who purchased the relevant book.

Document the decision.

---

# Phase 10 — Coupons and discounts

Potential model:

```text
Coupon
--------------------
Id
Code
DiscountType
DiscountValue
MinimumOrderAmount
StartDate
EndDate
UsageLimit
PerCustomerLimit
IsActive
```

Potential discount types:

```text
Percentage
FixedAmount
```

## Rules

- Coupon must be active.
- Current date must be valid.
- Usage limits enforced.
- Minimum order enforced.
- Per-customer limits enforced.
- Invalid coupons must provide a useful error message.

Pay attention to decimal/money handling and rounding.

---

# Phase 11 — Admin dashboard

Create an attractive, useful dashboard.

Metrics may include:

```text
Total revenue
Orders
Customers
Books
Low-stock books
Pending orders
Top-selling books
Recent orders
```

Charts may include:

- Sales over time.
- Orders over time.
- Top products.

Do calculations efficiently on the database side.

Avoid loading massive datasets simply to calculate one dashboard number in memory.

---

# Phase 12 — Validation

Implement multiple layers of validation.

## UI/model validation

Examples:

- Required fields.
- Length.
- Range.
- Email.
- Valid values.

## Server-side validation

Never trust client-side validation.

## Business validation

Examples:

```text
Quantity <= stock
Coupon not expired
Customer owns order
Review belongs to customer
Allowed order transition
Book is active
```

Keep business validation separate from simple UI validation when appropriate.

---

# Phase 13 — Error handling

Implement centralized error handling.

Handle:

```text
404
403
500
Expected business exceptions
Unexpected exceptions
```

Build user-friendly error pages.

Avoid exposing internal exception details in production.

---

# Phase 14 — Logging

Use:

```csharp
ILogger<T>
```

Log important events.

Examples:

```text
User registered
Order created
Order status changed
Inventory adjusted
Coupon applied
Payment failed
Unhandled exception
```

Use structured logging where practical.

Avoid replacing logging with random `Console.WriteLine()` calls.

---

# Phase 15 — Security

Cover at least:

- Authentication.
- Role-based authorization.
- Policy-based authorization.
- Antiforgery/CSRF protection.
- XSS prevention.
- SQL injection protection through parameterized ORM queries.
- Overposting protection through ViewModels.
- Secure file upload validation.
- Proper handling of secrets and connection strings.
- HTTPS in production.
- Appropriate cookie settings.
- Authorization checks on every sensitive operation.
- User ownership checks for customer data.

Security should be treated as part of feature development, not a final decoration.

---

# Phase 16 — UI/UX

The application should look like a real modern bookstore.

## Design system

Create consistent:

- Typography.
- Spacing.
- Buttons.
- Cards.
- Forms.
- Alerts.
- Badges.
- Tables.
- Modals.
- Navigation.
- Footer.

## Customer UI

- Responsive navbar.
- Search.
- Product cards.
- Book detail pages.
- Cart sidebar/page.
- Checkout flow.
- Account area.
- Orders.
- Wishlist.
- Empty states.
- Loading states.
- Error states.

## Admin UI

- Sidebar.
- Dashboard cards.
- Data tables.
- Search/filter controls.
- Forms.
- Status badges.
- Modals.

Use Bootstrap as the foundation where useful, then add custom CSS to make the project visually distinctive.

Do not over-engineer frontend tooling.

---

# Phase 17 — JavaScript/AJAX

Use JavaScript selectively to improve UX.

Potential AJAX features:

- Add-to-cart.
- Cart quantity changes.
- Wishlist toggling.
- Live search.
- Filter updates.
- Delete confirmations.
- Toast notifications.

Use normal MVC requests where AJAX provides no meaningful benefit.

Learn:

- `fetch`.
- JSON.
- HTTP status codes.
- Partial updates.
- Client/server responsibility.

---

# Phase 18 — Unit testing

Create unit tests for important business logic.

Examples:

```text
CartServiceTests
OrderServiceTests
CouponServiceTests
PricingServiceTests
InventoryServiceTests
```

Test:

- Valid behavior.
- Invalid behavior.
- Edge cases.
- Boundary conditions.
- Business rules.

Examples:

```text
Cannot order more than stock.
Expired coupon rejected.
Minimum order condition enforced.
Total calculated correctly.
Invalid status transition rejected.
```

Do not write tests only to increase coverage percentage. Test meaningful behavior.

---

# Phase 19 — Integration testing

Use ASP.NET Core integration testing to verify important application flows.

Potential tests:

```text
Register
Login
Book browsing
Admin book creation
Unauthorized admin access
Add to cart
Checkout
Order creation
Customer order ownership
```

Use a suitable test database strategy.

Do not duplicate every unit test at integration level.

---

# Phase 20 — Advanced EF Core

After core functionality is working, learn:

- Tracking vs `AsNoTracking`.
- Projection.
- Eager loading.
- Explicit loading where appropriate.
- Avoiding N+1 queries.
- Transactions.
- Concurrency.
- Indexes.
- Query performance.
- Global query filters where useful.
- Value conversions where useful.
- Shadow properties where useful.
- Database-side aggregation.
- Efficient pagination.
- Migration management.

Introduce advanced features only where they solve a real problem.

---

# Phase 21 — API layer

Expose selected functionality as APIs.

Potential endpoints:

```text
GET    /api/books
GET    /api/books/{id}
GET    /api/categories
GET    /api/orders
GET    /api/orders/{id}
```

Expand only where useful.

Learn:

- HTTP.
- REST.
- DTOs.
- JSON.
- Status codes.
- Validation.
- Authorization.
- API error responses.

Reuse application/business logic rather than duplicating it inside API controllers.

---

# Phase 22 — File storage

Implement robust image/file handling.

Requirements:

- File type validation.
- File size validation.
- Secure filenames.
- Unique filenames.
- Appropriate storage paths.
- Removal/replacement handling.
- Protection against malicious uploads.

Structure file storage behind an abstraction if this makes future storage changes easier.

---

# Phase 23 — Email

Potential emails:

```text
Welcome/registration
Password reset
Order confirmation
Order shipped
Order delivered
```

Start with development-friendly email configuration.

Keep email functionality behind an interface such as an application-facing abstraction.

---

# Phase 24 — Background processing

Introduce background processing for work that should not block the user request.

Potential examples:

```text
Send email
Generate reports
Cleanup jobs
Scheduled notifications
```

Explore:

- `BackgroundService`.
- Hosted services.
- Queues.
- Dependency scopes in background work.

Do not add a third-party job framework unless the project actually needs one.

---

# Phase 25 — Performance

Review:

## Database

- Indexes.
- Query count.
- Projection.
- Tracking.
- Pagination.
- Aggregation.

## Application

- Async operations.
- Caching.
- Avoid unnecessary allocations/work.
- Avoid duplicate queries.

## Frontend

- Image sizing.
- Static assets.
- Excessive HTTP requests.
- Rendering performance.

Do not optimize blindly. Identify the expensive operation first.

---

# Phase 26 — Caching

Introduce caching where repeated reads justify it.

Potential candidates:

- Categories.
- Authors.
- Popular books.
- Featured books.
- Some dashboard data.

Choose an appropriate ASP.NET Core caching approach and document invalidation behavior.

Do not cache rapidly changing or user-specific data casually.

---

# Phase 27 — Architectural refinement

After the application is functional, review the codebase.

Look for:

- Fat controllers.
- Duplicated logic.
- Large services.
- Unclear responsibilities.
- Unnecessary abstractions.
- Tight coupling.
- Inconsistent naming.
- Poor error handling.
- Missing authorization.
- N+1 queries.
- Inconsistent async usage.

Then refactor.

The goal is to learn that architecture evolves from real requirements.

---

# Phase 28 — Git and professional workflow

Use Git throughout the project.

Prefer meaningful commits.

Examples:

```text
feat: add book management
feat: implement customer storefront
feat: add shopping cart
feat: implement checkout flow
fix: prevent ordering unavailable stock
refactor: extract order pricing service
test: add checkout service tests
```

Use feature branches where appropriate:

```text
main
feature/book-management
feature/storefront
feature/cart
feature/checkout
feature/orders
feature/reviews
```

Keep commits focused.

---

# Phase 29 — Deployment

Prepare a production build.

Learn:

- Release configuration.
- Environment variables.
- Connection strings.
- Secrets.
- Production database.
- EF Core migrations.
- HTTPS.
- Logging.
- Publishing.
- Deployment.
- Error handling in production.

Potential production architecture:

```text
Client
  ↓
ASP.NET Core application
  ↓
SQL Server
```

Use an appropriate hosting provider based on availability, cost and the user's needs.

---

# Phase 30 — Final portfolio polish

Before considering BookShop finished:

## Code

- Clean naming.
- Consistent style.
- No dead code.
- No accidental debug code.
- No hard-coded secrets.
- No unnecessary comments.
- Meaningful abstractions.

## UI

- Responsive.
- Consistent.
- Accessible where practical.
- Clear error states.
- Clear empty states.
- Good forms.
- Good navigation.

## Documentation

README should include:

- Project overview.
- Features.
- Architecture.
- Technologies.
- Setup instructions.
- Database setup.
- Demo credentials for development only.
- Screenshots.
- API overview.
- Testing instructions.
- Deployment notes.

---

# 31. Final Expected Features

The completed BookShop should contain most or all of:

```text
✓ ASP.NET Core MVC
✓ .NET 8
✓ SQL Server
✓ Entity Framework Core 8
✓ ASP.NET Core Identity
✓ Authentication
✓ Role-based authorization
✓ Policy-based authorization
✓ Admin area
✓ Book management
✓ Author management
✓ Category management
✓ Publisher management
✓ Customer storefront
✓ Search
✓ Filtering
✓ Sorting
✓ Pagination
✓ Book details
✓ Shopping cart
✓ Wishlist
✓ Checkout
✓ Addresses
✓ Orders
✓ Order status management
✓ Inventory
✓ Reviews
✓ Ratings
✓ Coupons
✓ Discounts
✓ Admin dashboard
✓ Validation
✓ Error handling
✓ Logging
✓ File upload
✓ AJAX/JavaScript enhancements
✓ Unit tests
✓ Integration tests
✓ REST API
✓ Caching
✓ Background processing
✓ Email
✓ Performance optimization
✓ Security hardening
✓ Git workflow
✓ Deployment
✓ Professional README
```

---

# 32. Recommended Implementation Order

Follow this order unless a concrete dependency requires adjustment:

```text
01. Inspect existing solution
02. Stabilize current Identity/authentication/roles
03. Create layered architecture
04. Configure EF Core + SQL Server
05. Design domain
06. Create entities and configurations
07. Create migrations/database
08. Seed development data
09. Admin Book CRUD
10. Authors/Categories/Publishers
11. Customer storefront
12. Search/filter/sort/pagination
13. Book details
14. Shopping cart
15. Wishlist
16. Checkout
17. Orders
18. Inventory
19. Reviews/ratings
20. Coupons/discounts
21. Admin dashboard
22. Validation
23. Error handling
24. Logging
25. Security hardening
26. UI/UX refinement
27. JavaScript/AJAX
28. Unit tests
29. Integration tests
30. Advanced EF Core improvements
31. API
32. File storage refinement
33. Email
34. Background jobs
35. Caching
36. Performance review
37. Architecture/code review
38. Git cleanup
39. Deployment
40. Documentation/portfolio polish
```

---

# 33. Definition of Done for a Feature

A feature is not considered complete just because its happy-path UI works.

For each meaningful feature, verify:

```text
[ ] Domain model is correct
[ ] Database relationships are correct
[ ] EF Core configuration is correct
[ ] Migration/database changes work
[ ] Business rules are implemented
[ ] Validation exists
[ ] Authorization exists where necessary
[ ] User ownership is enforced where necessary
[ ] Error cases are handled
[ ] Logging is appropriate
[ ] Async operations are used appropriately
[ ] Queries are reasonably efficient
[ ] UI is responsive and consistent
[ ] Empty/error states exist
[ ] Important business behavior has tests
[ ] Code is refactored if necessary
```

---

# 34. Agent Working Instructions

The coding agent should follow these rules throughout development.

## Rule 1 — Inspect before changing

Before modifying existing files:

- Read the relevant code.
- Understand current behavior.
- Preserve working functionality.
- Avoid unnecessary rewrites.

## Rule 2 — Stay on .NET 8

Do not upgrade the project framework unless explicitly requested.

## Rule 3 — Do not fabricate existing code

If a file, class, service or configuration does not exist, inspect the actual repository and create it intentionally.

## Rule 4 — Minimize unnecessary changes

When implementing a feature, modify only what is needed.

## Rule 5 — Prefer explicit, understandable code

The project is a learning project.

Code should be professional but understandable.

Avoid excessive cleverness.

## Rule 6 — Explain architectural choices

For significant design decisions, briefly explain why the design was chosen.

## Rule 7 — Do not introduce patterns for decoration

Only introduce Repository, Unit of Work, Specification, Factory, Strategy, Mediator, CQRS, etc. when there is a concrete reason.

## Rule 8 — Keep controllers thin

Controllers should coordinate requests and responses.

Business logic should live in appropriate services/domain/application components.

## Rule 9 — Do not expose entities directly when a ViewModel/DTO is more appropriate

Especially for:

- Create/update forms.
- Admin forms.
- API responses.
- Complex pages.

## Rule 10 — Security is mandatory

Every new feature must be checked for:

- Authentication requirements.
- Authorization.
- Ownership.
- Input validation.
- Overposting.
- CSRF.
- File upload risks.
- Sensitive data exposure.

## Rule 11 — Do not stop at the happy path

For every major feature, consider:

- Invalid input.
- Missing records.
- Unauthorized access.
- Concurrent changes.
- Empty collections.
- Boundary values.
- Database failures.
- Business rule violations.

## Rule 12 — Verify builds and tests

After meaningful changes:

1. Build the solution.
2. Run relevant tests.
3. Fix errors caused by the implementation.
4. Keep the repository in a runnable state.

## Rule 13 — Do not hide errors

Do not suppress compiler warnings or exceptions simply to make the build pass.

## Rule 14 — Keep database logic efficient

Avoid:

- N+1 queries.
- Unnecessary `Include`.
- Loading entire tables for simple counts.
- Client-side filtering where database-side filtering is appropriate.

## Rule 15 — Keep the UI professional

Do not treat UI as an afterthought.

Each customer-facing feature should have:

- Responsive design.
- Loading/empty/error states where relevant.
- Clear validation.
- Consistent components.
- Good visual hierarchy.

---

# 35. Teaching Mode

The project should also serve as the user's .NET curriculum.

When a new concept appears, explain it in context.

Examples:

### When introducing DI

Explain:

```text
What dependency injection is
Why it exists
How ASP.NET Core implements it
How services are registered
How controllers receive dependencies
```

### When introducing EF Core

Explain:

```text
DbContext
DbSet
tracking
migrations
relationships
LINQ translation
SQL generation
```

### When introducing services

Explain:

```text
Why controller logic is becoming too large
What responsibility belongs in a service
What belongs in the domain
What belongs in infrastructure
```

### When introducing testing

Explain:

```text
What is being tested
Why it is unit vs integration
What should not be tested directly
```

The objective is to build transferable understanding, not memorization.

---

# 36. Final Learning Outcome

By the end of BookShop, the user should be able to independently create and explain an ASP.NET Core MVC application containing:

```text
Browser
  ↓
HTTP
  ↓
ASP.NET Core middleware
  ↓
MVC routing
  ↓
Controller
  ↓
Model binding / validation
  ↓
Application service
  ↓
Domain/business rules
  ↓
EF Core
  ↓
SQL Server
  ↓
Response/View/API
```

The user should also understand how to:

- Design a relational database.
- Structure a .NET solution.
- Use EF Core confidently.
- Build secure MVC applications.
- Design business logic.
- Build role-based systems.
- Write tests.
- Diagnose errors.
- Optimize database queries.
- Build APIs.
- Deploy applications.
- Explain architectural decisions in an interview.

---

# 37. Start Here

The next task is **Phase 0**.

Before implementing anything:

1. Inspect the existing BookShop solution.
2. Confirm it targets .NET 8.
3. Identify all current projects.
4. Identify the current authentication/Identity implementation.
5. Identify how roles are currently created/seeded.
6. Identify the current DbContext.
7. Identify the current SQL Server configuration.
8. Confirm the application builds and runs.
9. Propose the smallest set of architectural changes required to reach the target structure.
10. Do not overwrite working authentication code without a reason.

Then begin implementing the foundation before moving to Book Management.

The agent should maintain a clear record of:

- Current phase.
- Completed tasks.
- Remaining tasks.
- Important architectural decisions.
- Database changes.
- Known technical debt.

Do not jump ahead across multiple phases without ensuring the current phase is understood and stable.
