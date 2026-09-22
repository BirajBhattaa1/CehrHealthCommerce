# Report Evidence Checklist — CSC381 E-Commerce Project

Use this as a walkthrough when writing your project report and preparing your demo/viva. For each
of the five required areas it lists **what to claim**, **which files to cite as evidence**, and
**what to capture (screenshot / live demo)**.

> Reminder for the report's introduction: state clearly that the **NID is simulated/local**, that
> **payments use the eSewa sandbox**, and that **all data is fictional**. This framing is honest and
> is expected by the rubric.

---

## Area 1 — E-commerce website

**Claim:** A complete storefront: product catalogue with categories, product detail pages, a
shopping cart, checkout with a Nepal address (province/district/municipality), order placement and
order history; plus an admin panel to manage products, categories, orders and users.

**Cite these files**

- Catalogue & detail: `Controllers/ProductsController.cs`, `Services/ProductService.cs`,
  `Views/Products/List.cshtml`, `Views/Products/Details.cshtml`
- Cart: `Controllers/CartController.cs`, `Services/CartService.cs`, `Views/Cart/Index.cshtml`
- Checkout & orders: `Controllers/CheckoutController.cs`, `Controllers/OrdersController.cs`,
  `Services/OrderService.cs`, `Data/NepalGeoData.cs`
- Admin: `Areas/Admin/Controllers/*`, `Areas/Admin/Views/*`
- Data model: `Models/*`, `Data/ApplicationDbContext.cs`, `Data/DbSeeder.cs`

**Capture**

1. Home page with featured products.
2. A category/listing page and a product detail page (show the "You may also like" strip).
3. Cart with 2–3 items and the order summary.
4. Checkout form with the cascading Province → District → Municipality selectors.
5. Order confirmation page and the order history list.
6. Admin dashboard + the product list (with the create/edit form).

---

## Area 2 — Payment gateway (eSewa sandbox)

**Claim:** Integrated the eSewa ePay v2 gateway (sandbox). The order total is signed with
**HMAC-SHA256** before redirecting; the callback is **verified** by recomputing the signature over
`signed_field_names` and comparing in constant time before the order is marked paid.

**Cite these files**

- `Services/EsewaPaymentService.cs` (BuildForm + VerifyAsync — signing & verification)
- `Services/EsewaOptions.cs`, `Controllers/PaymentController.cs`, `Views/Payment/Pay.cshtml`
- Config: `appsettings.json` → `Esewa` section (public sandbox `EPAYTEST` values)

**Capture**

1. The "redirecting to eSewa" page (`/Payment/Pay`).
2. The eSewa sandbox payment screen.
3. The successful order confirmation (status → *Paid*).
4. *(Strong evidence)* A tampered-callback test: call `/Payment/Success?data=<garbage>` and show the
   order is **not** marked paid — see test **T7** in `docs/SECURITY.md`.

---

## Area 3 — SEO & analytics

**Claim:** Implemented technical + on-page SEO — a dynamic XML sitemap, a robots.txt, canonical and
Open Graph tags, and per-page meta descriptions. Google Analytics 4 is wired up but only activates
when a real Measurement ID is configured (so no fake analytics data is generated for the demo).

**Cite these files**

- `Controllers/SeoController.cs` (`/sitemap.xml`, `/robots.txt`)
- `Views/Shared/_Layout.cshtml` (title, meta description, canonical, Open Graph)
- `Views/Shared/_GoogleAnalytics.cshtml` (conditional GA4 tag)
- Per-page meta descriptions set in `Controllers/ProductsController.cs` (`ViewData["MetaDescription"]`)

**Capture**

1. Browser view of `/sitemap.xml` (shows home, sections, categories and product URLs).
2. Browser view of `/robots.txt` (Allow public, Disallow private areas, Sitemap line).
3. Page source of a product page showing `<title>`, `<meta name="description">`, `<link rel="canonical">`
   and `og:` tags.
4. *(Optional)* Explain in text that setting `Analytics:GoogleMeasurementId` injects the GA4 tag;
   show the `_GoogleAnalytics.cshtml` conditional as evidence of the honest default-off behaviour.

---

## Area 4 — Recommendation system

**Claim:** A content-based recommender suggests related products from the **same category**, falling
back to the **same product type**, shown on product detail pages and as a cart cross-sell.

**Cite these files**

- `Services/ContentBasedRecommendationService.cs` (the algorithm)
- `Services/IRecommendationService.cs`
- `ViewComponents/RelatedProductsViewComponent.cs`,
  `Views/Shared/Components/RelatedProducts/Default.cshtml`
- Surfaces: `Views/Products/Details.cshtml`, `Views/Cart/Index.cshtml`

**Capture**

1. A product detail page showing *You may also like* with same-category products.
2. The cart page showing *Recommended for you*.
3. In the report, briefly explain the logic (same category → same type fallback) and why
   content-based filtering suits a fresh catalogue with no purchase history yet.

---

## Area 5 — Security testing

**Claim:** Implemented and tested defences against the common web threats — CSRF, XSS, SQL
injection, clickjacking, insecure transport, broken authentication and access control, payment
tampering/forgery, and unsafe file upload.

**Cite this file (primary evidence)**

- **`docs/SECURITY.md`** — a 17-row control table (threat → control → location) and a **12-test
  matrix (T1–T12)** with steps and expected results.
- Supporting: `Program.cs` (anti-forgery filter, security headers/CSP, HTTPS/HSTS, Identity policy),
  every controller (`[ValidateAntiForgeryToken]`, `[Authorize]`).

**Capture**

Run at least these tests from `docs/SECURITY.md` and screenshot the result of each:

1. **T1** — SQL-injection string in search is treated as literal text (no error/dump).
2. **T2** — removing the anti-forgery token yields **400 Bad Request**.
3. **T3** — a `<script>` full name renders escaped (no alert box) — XSS blocked.
4. **T4/T5** — access control: a user can't open another user's order; a customer can't reach `/Admin`.
5. **T7** — tampered payment callback is rejected.
6. **T10** — response headers show `Content-Security-Policy`, `X-Frame-Options`, etc.

---

## Suggested report structure

1. Introduction & scope (with the simulated-NID / sandbox / fictional-data disclaimer).
2. Background: Nepal e-governance → CEHR → citizen health e-commerce.
3. System design: architecture (MVC), data model (ER diagram from `Models/`), workflow.
4. Implementation of each of the five areas (use the sections above).
5. Security testing results (the T1–T12 table with your screenshots).
6. Limitations & future work (see README §10 and `docs/SECURITY.md` §3).
7. Conclusion.
