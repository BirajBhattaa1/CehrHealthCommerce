# Security Implementation & Test Matrix — Nepal MediHub

This document is the evidence for **Area 5: Security Testing** of the CSC381 project.
It lists the security controls built into the application, where each one lives in the
source code, and a repeatable manual test for each.

> Scope note: this is a course demo. Citizen NID identity is **simulated / local only**
> and is never linked to any real Government of Nepal system. Payments use the **eSewa
> sandbox** only — no real money moves. No real patient, personal, or payment data is used.

---

## 1. Security controls implemented

| # | Threat | Control | Where in code |
|---|--------|---------|---------------|
| 1 | SQL Injection | All data access uses EF Core LINQ (parameterised queries). No string-concatenated SQL, no raw SQL. | All `Services/*Service.cs` |
| 2 | Cross-Site Request Forgery (CSRF) | Global `AutoValidateAntiforgeryToken` filter + explicit `[ValidateAntiForgeryToken]` on every POST action. Anti-forgery token auto-emitted by the form tag helper. | `Program.cs`, all controllers |
| 3 | Cross-Site Scripting (XSS) | Razor HTML-encodes all output by default. `HttpOnly` auth cookie is not readable by JS. Content-Security-Policy restricts script sources. | Razor views, `Program.cs` |
| 4 | Clickjacking | `X-Frame-Options: DENY` and CSP `frame-ancestors 'none'`. | `Program.cs` security headers |
| 5 | MIME sniffing | `X-Content-Type-Options: nosniff`. | `Program.cs` security headers |
| 6 | Man-in-the-middle | `UseHttpsRedirection()` + HSTS (`UseHsts()` in production). Auth cookie is `Secure` (HTTPS only). | `Program.cs` |
| 7 | Broken authentication | ASP.NET Core Identity with a password policy (length, upper/lower/digit) and account lockout after 5 failed attempts. | `Program.cs` Identity options |
| 8 | Broken access control (horizontal) | Order reads are always filtered by the signed-in `userId`, so a user cannot open another user's order by changing the id. | `OrderService.GetForUserAsync`, `OrdersController` |
| 9 | Broken access control (vertical) | Admin area is protected with `[Authorize(Roles = "Admin")]` on every admin controller; area is also `noindex`. | `Areas/Admin/Controllers/*`, `_AdminLayout` |
| 10 | Payment tampering | Order totals are always recomputed server-side from live product prices — the client never supplies an amount. | `OrderService.CreateFromCartAsync` |
| 11 | Payment forgery | eSewa response is verified with an HMAC-SHA256 signature over the signed fields, compared in constant time, plus a best-effort status-API confirmation. | `EsewaPaymentService.VerifyAsync` |
| 12 | Secret leakage | No real secrets in source. eSewa **sandbox** test key lives in configuration; production keys are expected via user-secrets / environment variables. | `appsettings.json`, `EsewaOptions` |
| 13 | Sensitive data exposure | Passwords hashed by Identity (PBKDF2). NID is simulated and not a primary key. No real PII stored. | Identity, `ApplicationUser` |
| 14 | Information leak via errors | Production uses a generic error page (`/Home/Error`); stack traces are not shown to end users. | `Program.cs`, `Views/Shared/Error.cshtml` |
| 15 | Malicious file upload | Product image upload restricts extension (jpg/jpeg/png/webp/gif) and size (≤ 2 MB); files are saved with a generated GUID name. | `Areas/Admin/Controllers/ProductsController` |
| 16 | Over-posting / mass assignment | Controllers bind to dedicated ViewModels, not EF entities directly, for create/edit forms. | `ViewModels/Admin/*`, `CheckoutViewModel` |
| 17 | Brute-force / self lock-out | Admin accounts cannot be locked from the Users screen (prevents locking the only admin out). | `Areas/Admin/Controllers/UsersController` |
| 18 | API-level IDOR (medical history) | The medical-history endpoint takes **no** user identifier; the subject is read from the signed-in principal. An authenticated user cannot read another citizen's NID or purchase history. | `Controllers/ApiController.cs` |
| 19 | Credential / resource flooding | Fixed-window rate limiting: a global **300 req/min per IP** budget, plus tighter per-endpoint policies — `login` **10/min**, `register` **5 per 10 min**, `payment` **20/min**. Excess requests get **429**. | `Program.cs`, `AccountController`, `PaymentController` |

---

## 2. Manual test matrix

Run these against the app on `https://localhost:<port>`. Expected results confirm the control works.

| Test | Steps | Expected result |
|------|-------|-----------------|
| T1 — SQLi in search | In product search type `' OR '1'='1` | Treated as literal text; returns products matching that string (usually none). No error, no data dump. |
| T2 — CSRF token required | Submit any POST form with the hidden `__RequestVerificationToken` removed (via dev tools) | Server responds **400 Bad Request**. |
| T3 — Stored/reflected XSS | Register with full name `<script>alert(1)</script>`, view it on the profile / admin users list | Text is shown escaped; **no alert box** appears. |
| T4 — Horizontal access control | Log in as user A, note an order id, then log in as user B and browse to `/Orders/Details/<A's id>` | **404 Not Found** (B cannot see A's order). |
| T5 — Vertical access control | As a normal customer, browse to `/Admin/Dashboard` | Redirected to **Access Denied** / login. |
| T6 — Price tampering | On the cart/checkout page, edit a hidden price field (dev tools) and submit | Order total is recomputed server-side; tampered value is ignored. |
| T7 — Payment signature | Call `/Payment/Success?data=<tampered base64>` | "Payment could not be verified" — order is **not** marked paid. |
| T8 — Account lockout | Enter a wrong password 5 times for one account | Account is locked for the configured period. |
| T9 — HTTPS redirect | Browse to the `http://` URL | Redirected to `https://`. |
| T10 — Security headers | Inspect any response headers (dev tools → Network) | `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Content-Security-Policy` present. |
| T11 — File upload filter | In admin, try to upload a `.exe` (or a 5 MB image) as a product image | Rejected with a validation message. |
| T12 — Error handling | Trigger an error in production mode | Generic error page shown; no stack trace / no SQL details. |
| T13 — Login rate limit | POST to `/Account/Login` more than 10 times in a minute from one IP | First 10 are processed; the rest return **429 Too Many Requests**. |
| T14 — Medical-history IDOR | While logged in as user B, request `/api/user/me/medical-history` | Returns **only B's own** records. Requesting someone else's id is not possible — the route accepts no id, and the old `/api/user/{id}/medical-history` shape now returns **404**. |

---

## 3. Known demo limitations (documented on purpose)

- Stock is decremented when an order is placed (including for eSewa orders awaiting payment),
  so an abandoned eSewa payment holds stock until the order is cancelled. This is a deliberate
  simplification for the demo.
- CSP uses `'unsafe-inline'` for scripts/styles because the UI has small inline blocks. A
  production hardening step would move these to files and use nonces/hashes.
- The eSewa status-API confirmation is best-effort; the authoritative check is the HMAC
  signature on the returned payload.
