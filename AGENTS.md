# Gwire

- .NET 10 Blazor WebAssembly app for guitar-wiring diagrams.
- Keep changes scoped; do not edit `bin/` or `obj/`.
- Use component-scoped `.razor.css` for page-specific styles and
  `wwwroot/css/app.css` for global styles.
- Put Blazor component logic in a code-behind `.razor.cs` file. Keeping a few
  properties, a short method, or similarly small code directly in a `.razor`
  file is acceptable when a separate file would add unnecessary overhead.
- Preserve CRLF line endings.
- Verify changes with `dotnet build gwire.slnx --no-restore`.
- using css Boostrap v5.3.3 and Font Awesome Free 7.3.1
- Use en.us for code, comments and UI