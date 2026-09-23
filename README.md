# CEHR Health — Health E-Commerce Platform for Nepal

A health-focused online store (medicines, medical equipment and health products) built for the
**BSc CSIT 6th-semester CSC381 — E-Commerce** course. The project is set in the context of
Nepal's e-governance vision and a proposed **Centralized Electronic Health Record (CEHR)**: a
citizen signs up with a (simulated) National ID, and can then browse, add to cart, checkout, pay
through the **eSewa sandbox**, and track orders.

> **Academic project disclaimer.** This is course work. The **NID is a simulated, local-only
> identifier** and is **not** connected to any Government of Nepal system or any real CEHR. All
> catalogue data, users, prices and orders are **fictional/sample**. Payments run against the
> **eSewa sandbox (test) environment only** — no real money moves. This is **not** a licensed
> pharmacy and performs **no** legal prescription validation.

---

## 1. How this maps to the five CSC381 areas

| # | Required area | Where it is implemented | How to demonstrate |
|---|---------------|-------------------------|--------------------|
| 1 | **E-commerce website** | Full catalogue, cart, checkout, orders (`Controllers/`, `Views/`, `Services/`) | Browse → add to cart → checkout → order confirmation |
| 2 | **Payment gateway** | eSewa ePay v2 sandbox with HMAC-SHA256 signature + verification (`Services/EsewaPaymentService.cs`, `Controllers/PaymentController.cs`) | Place an order, pay with the eSewa **test** credentials |
| 3 | **SEO & analytics** | Dynamic `sitemap.xml` + `robots.txt`, canonical & Open Graph tags, per-page meta descriptions, optional Google Analytics 4 (`Controllers/SeoController.cs`, `Views/Shared/_GoogleAnalytics.cshtml`, `_Layout.cshtml`) | Open `/sitemap.xml` and `/robots.txt`; view page source for meta tags |
| 4 | **Recommendation system** | Content-based "related products" (same category, then same product type) surfaced on product and cart pages (`Services/ContentBasedRecommendationService.cs`, `ViewComponents/RelatedProductsViewComponent.cs`) | Open any product; scroll to *You may also like* |
| 5 | **Security testing** | CSRF, XSS, SQLi-safe EF, security headers/CSP, HTTPS, Identity + lockout, access control (`Program.cs`, all controllers) — full evidence in [`docs/SECURITY.md`](docs/SECURITY.md) | Run the 12-test matrix in `docs/SECURITY.md` |

---

## 2. Technology stack

- **ASP.NET Core MVC** on **.NET 8 (LTS)** — C#, Razor views
- **Entity Framework Core 8** (Code-First + Migrations) with **SQL Server LocalDB**
- **ASP.NET Core Identity** — roles (`Admin`, `Customer`), password policy, account lockout
- **Bootstrap 5.3** + Bootstrap Icons + jQuery (via CDN)
- **eSewa ePay v2** sandbox payment gateway

This is a standard ASP.NET Core MVC project (`Microsoft.NET.Sdk.Web`). It opens directly in
**Visual Studio 2022** (open `CehrHealthCommerce.csproj`) or runs from the **command line** with the
.NET CLI — no VS Code-specific or non-standard project files are used.

---

## 3. Prerequisites

Install the following on Windows:

1. **.NET 8 SDK** — https://dotnet.microsoft.com/download/dotnet/8.0
2. **SQL Server LocalDB** — included with Visual Studio 2022 (any edition) or the
   *SQL Server Express* installer ("LocalDB" feature). The default connection string uses
   `(localdb)\MSSQLLocalDB`.
3. *(Optional)* **Visual Studio 2022** — for an IDE experience; not required if you use the CLI.


## 7. SEO & analytics

- **Sitemap:** `GET /sitemap.xml` — generated dynamically from active categories and products.
- **Robots:** `GET /robots.txt` — allows the public storefront, disallows private areas
  (`/Admin`, `/Account`, `/Cart`, `/Checkout`, `/Orders`, `/Payment`), and links the sitemap.
- **On-page SEO:** each page sets a `<title>`, `<meta name="description">`, a canonical URL and
  Open Graph tags (`_Layout.cshtml`); product/category pages set descriptive meta text.
- **Google Analytics (optional & honest):** the GA4 tag renders **only** when a real Measurement ID
  is configured. By default `Analytics:GoogleMeasurementId` is empty, so **no** analytics script is
  injected and no fake traffic is generated. To enable, set a real `G-XXXXXXXXXX` ID via
  user-secrets, an environment variable (`Analytics__GoogleMeasurementId`), or configuration.

---

## 8. Security & configuration

Security controls (CSRF, XSS, SQL-injection-safe EF, security headers/CSP, HTTPS + HSTS, Identity
password policy + lockout, role-based access control, server-side price recomputation, signed
payment verification) are documented with a **repeatable test matrix** in
[`docs/SECURITY.md`](docs/SECURITY.md) — this is the evidence for Area 5.

### Configuration keys (`appsettings.json`)

| Key | Purpose | Default |
|-----|---------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server LocalDB connection | `(localdb)\MSSQLLocalDB` → `CehrHealthCommerceDb` |
| `Esewa:ProductCode` | eSewa merchant code | `EPAYTEST` (public sandbox code) |
| `Esewa:SecretKey` | eSewa signing key | eSewa's **public** sandbox test key |
| `Esewa:FormUrl` / `Esewa:StatusUrl` | eSewa sandbox endpoints | rc-epay / rc sandbox URLs |
| `Analytics:GoogleMeasurementId` | GA4 Measurement ID | *empty* (analytics off) |

> **Never put real secrets in source.** The values above are eSewa's **published sandbox test
> values**, safe to commit for a demo. For a real deployment, move production keys to
> **user-secrets** (`dotnet user-secrets set "Esewa:SecretKey" "…"`) or **environment variables**.

---

## 9. Project structure

```
CehrHealthCommerce/
├─ Controllers/            # Home, Products, Cart, Checkout, Orders, Payment, Account, Seo
├─ Areas/Admin/            # Admin panel (Dashboard, Products, Categories, Orders, Users)
├─ Models/                 # EF entities (Product, Category, Order, Payment, ApplicationUser, …)
├─ Data/                   # ApplicationDbContext, DbSeeder, NepalGeoData
├─ Services/               # Business logic (catalogue, cart, orders, recommendations, eSewa)
├─ ViewComponents/         # CartSummary, RelatedProducts
├─ ViewModels/             # Form/display view models (incl. Admin/)
├─ Views/                  # Razor views + shared layout, _GoogleAnalytics, _ProductCard
├─ wwwroot/                # site.css, site.js, uploaded product images
├─ docs/SECURITY.md        # Security controls + test matrix (Area 5 evidence)
├─ appsettings.json        # Configuration (DB, eSewa sandbox, analytics)
└─ Program.cs              # DI, security headers/CSP, middleware, routing, startup seeding
```

---

## 10. Notes, scope & limitations

- **Simulated NID only** — a local unique identifier for the demo; it is **not** a database primary
  key and is **not** linked to any government system.
- **eSewa sandbox only** — no real payments; signature verification is implemented for realism.
- **Not a pharmacy system** — the `PrescriptionRequired` flag is informational; there is no legal
  prescription validation or dispensing workflow.
- **No fake external data** — there are no fake government APIs, and analytics stays off unless a
  real GA4 ID is supplied.
- All catalogue data is **fictional**; prices are illustrative and in **NPR**.

---

*Built for CSC381 (E-Commerce), BSc CSIT — Tribhuvan University syllabus. For academic use By Biraj Bhatta, Lila Katuwal and Basant Raj Kadel.*
