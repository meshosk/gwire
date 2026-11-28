# Vendored libraries

Third-party front-end files are copied by hand into `Gwire/wwwroot/lib/` and referenced from
`Gwire/wwwroot/index.html`. There is no libman, npm, or CDN; do not add one.

| Library | Version | Files | Note |
| --- | --- | --- | --- |
| Bootstrap | 5.3.3 | `lib/bootstrap/dist/` | CSS and `bootstrap.bundle.min.js`. |
| Font Awesome Free | 7.3.1 | `lib/fontawesome/all.min.css`, `lib/webfonts/*.woff2` | CSS loads `../webfonts/`. |
| Material Design Icons | 7.4.47 | `lib/mdi/css/materialdesignicons.min.css`, `lib/mdi/fonts/materialdesignicons-webfont.woff2`, `lib/mdi/LICENSE` | Only for electronic symbols. CSS loads `../fonts/`. Has `mdi-resistor`, `mdi-knob`; no capacitor. |
| jsPDF | 4.2.1 | `lib/jspdf/jspdf.umd.min.js` | PDF export. |

## Adding or updating a library

1. Download the exact version from the npm registry, for example
   `https://registry.npmjs.org/@mdi/font/-/font-7.4.47.tgz`, and extract it outside the repository.
2. Copy only what the app uses, keeping the relative paths the CSS expects for fonts. Fonts:
   only `.woff2` (no `.eot`, `.ttf`, `.woff`, maps, or sources). Keep the license file.
3. Verify the copy (for example `sha256sum` against the package) and that the icon names you use
   exist: `grep -c '\.mdi-resistor::before' lib/mdi/css/materialdesignicons.min.css`.
4. Add the `<link rel="stylesheet">` or `<script>` in `index.html` next to the other libraries,
   stylesheets before `css/app.css`.
5. Update the table above and the quick facts in [index.md](index.md).
6. Hard-reload the app; the PWA service worker may serve the old `index.html` from cache.
