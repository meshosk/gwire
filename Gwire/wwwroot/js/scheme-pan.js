window.gwire = window.gwire || {};

(() => {
    /** Owns the viewport, pan mode, and event subscriptions for one scheme editor. */
    class SchemePan {
        static minZoom = 0.2;
        static maxZoom = 2;

        zoom = 1;
        panEnabled = false;
        spaceHeld = false;
        pointerId = null;
        lastX = 0;
        lastY = 0;
        x = 0;
        y = 0;
        suppressClick = false;

        constructor(editor, receiver) {
            this.editor = editor;
            this.receiver = receiver;
            this.svg = editor.querySelector(".scheme-editor-canvas");
            this.toggle = editor.querySelector("[data-scheme-pan-toggle]");
            this.icon = this.toggle.querySelector("i");
            this.grid = this.svg.querySelector('rect[fill="url(#scheme-grid)"]');
            this.controller = new AbortController();

            const options = { signal: this.controller.signal };
            // Intercept pan gestures before the editor's part-selection and drag handlers.
            const captureOptions = { ...options, capture: true };
            this.toggle.addEventListener("click", this.toggleMode, options);
            window.addEventListener("keydown", this.handleKeyDown, options);
            window.addEventListener("keyup", this.handleKeyUp, options);
            window.addEventListener("blur", this.releaseSpace, options);
            this.svg.addEventListener("pointerdown", this.startPan, captureOptions);
            this.svg.addEventListener("pointermove", this.movePan, captureOptions);
            this.svg.addEventListener("pointerup", this.endPan, captureOptions);
            this.svg.addEventListener("pointercancel", this.endPan, captureOptions);
            this.svg.addEventListener("lostpointercapture", this.endPan, captureOptions);
            this.svg.addEventListener("click", this.handleClick, captureOptions);
            this.svg.addEventListener("wheel", this.handleWheel, { ...options, passive: false });

            // At 100%, one scheme unit equals one CSS pixel, including after resizing.
            this.svg.setAttribute("preserveAspectRatio", "none");
            this.resizeObserver = new ResizeObserver(() => this.updateView());
            this.resizeObserver.observe(this.svg);
            this.updateView();
            this.updateMode();
        }

        // Arrow-function handlers retain this instance when called by DOM events.
        toggleMode = () => {
            if (!this.spaceHeld) {
                this.panEnabled = !this.panEnabled;
                this.updateMode();
            }
        };

        /** Space temporarily inverts the mode chosen with the toolbar button. */
        isPanMode() {
            return this.panEnabled !== this.spaceHeld;
        }

        updateMode() {
            const panning = this.isPanMode();
            this.svg.classList.toggle("pan-ready", panning);
            this.svg.classList.toggle("panning", panning && this.pointerId !== null);
            this.toggle.classList.toggle("active", panning);
            this.toggle.disabled = this.spaceHeld;
            this.toggle.setAttribute("aria-pressed", String(panning));
            this.toggle.title = panning ? "Pan mode (hold Space to edit)" : "Edit mode (hold Space to pan)";
            this.icon.classList.toggle("fa-computer-mouse", !panning);
            this.icon.classList.toggle("fa-up-down-left-right", panning);
        }

        handleKeyDown = (event) => {
            // Leave text input, modified shortcuts, and unrelated controls to their own handlers.
            if (event.code !== "Space" || event.ctrlKey || event.metaKey || event.altKey ||
                event.target.closest("input, textarea, select, [contenteditable]") ||
                (event.target.closest("button, a") && event.target.closest("[data-scheme-pan-toggle]") !== this.toggle &&
                 !this.svg.matches(":hover"))) {
                return;
            }
            if (this.editor.contains(document.activeElement) || this.svg.matches(":hover")) {
                event.preventDefault();
            }
            this.spaceHeld = true;
            this.updateMode();
        };

        handleKeyUp = (event) => {
            if (event.code === "Space") {
                this.releaseSpace();
            }
        };

        // Also reset on window blur, since the corresponding keyup may happen elsewhere.
        releaseSpace = () => {
            this.spaceHeld = false;
            this.updateMode();
        };

        /** Move the viewport and grid together so the background covers the visible area. */
        updateView() {
            const width = this.svg.clientWidth / this.zoom;
            const height = this.svg.clientHeight / this.zoom;
            this.svg.setAttribute("viewBox", `${this.x} ${this.y} ${width} ${height}`);
            this.grid.setAttribute("x", this.x);
            this.grid.setAttribute("y", this.y);
        }

        /** Zoom around the cursor, keeping its scheme position fixed on screen. */
        handleWheel = (event) => {
            event.preventDefault();
            // Wheel deltas can be expressed in pixels, lines, or pages.
            const deltaUnit = event.deltaMode === 1 ? 16 : event.deltaMode === 2 ? this.svg.clientHeight : 1;
            const zoom = this.zoom * Math.exp(-event.deltaY * deltaUnit * 0.001);
            const bounds = this.svg.getBoundingClientRect();
            this.setZoom(zoom, event.clientX - bounds.left, event.clientY - bounds.top);
        };

        /** Toolbar changes zoom around the viewport center; the wheel supplies the cursor position. */
        setZoom(zoom, cursorX = this.svg.clientWidth / 2, cursorY = this.svg.clientHeight / 2) {
            zoom = Math.min(SchemePan.maxZoom, Math.max(SchemePan.minZoom, zoom));
            if (this.pointerId !== null || zoom === this.zoom) {
                return;
            }

            // WebAssembly interop is synchronous so C# receives the scale before the next drag.
            // The editor rejects zoom changes while a part or cable point is being dragged.
            if (!this.receiver.invokeMethod("HandleZoomChanged", zoom)) {
                return;
            }

            this.x += cursorX / this.zoom - cursorX / zoom;
            this.y += cursorY / this.zoom - cursorY / zoom;
            this.zoom = zoom;
            this.updateView();
        }

        startPan = (event) => {
            this.suppressClick = false;
            if (!this.isPanMode() || event.button !== 0 || !event.isPrimary || this.pointerId !== null) {
                return;
            }
            this.pointerId = event.pointerId;
            this.lastX = event.clientX;
            this.lastY = event.clientY;
            this.suppressClick = true;
            this.editor.focus({ preventScroll: true });
            // Continue receiving this gesture even when the pointer leaves the canvas.
            this.svg.setPointerCapture(this.pointerId);
            this.svg.classList.add("panning");
            event.preventDefault();
            event.stopImmediatePropagation();
        };

        movePan = (event) => {
            if (event.pointerId !== this.pointerId) {
                return;
            }
            if (this.isPanMode()) {
                this.x -= (event.clientX - this.lastX) / this.zoom;
                this.y -= (event.clientY - this.lastY) / this.zoom;
                this.updateView();
            }
            // Track movement while Space switches modes to avoid a jump when panning resumes.
            this.lastX = event.clientX;
            this.lastY = event.clientY;
            event.preventDefault();
            event.stopImmediatePropagation();
        };

        endPan = (event) => {
            if (event.pointerId !== this.pointerId) {
                return;
            }
            this.pointerId = null;
            this.svg.classList.remove("panning");
            if (this.svg.hasPointerCapture(event.pointerId)) {
                this.svg.releasePointerCapture(event.pointerId);
            }
            event.stopImmediatePropagation();
        };

        handleClick = (event) => {
            // A completed pan gesture must not trigger a part-selection click.
            if (this.suppressClick) {
                this.suppressClick = false;
                event.preventDefault();
                event.stopImmediatePropagation();
            }
        };

        /** Release subscriptions and pointer capture when Blazor disposes the editor. */
        dispose() {
            this.controller.abort();
            this.resizeObserver.disconnect();
            if (this.pointerId !== null && this.svg.hasPointerCapture(this.pointerId)) {
                this.svg.releasePointerCapture(this.pointerId);
            }
            this.releaseSpace();
        }
    }

    // Blazor entry points share the same viewport controller for wheel and toolbar zoom.
    window.gwire.enableSchemePan = (editor, receiver) => {
        editor.gwireSchemePan = new SchemePan(editor, receiver);
    };

    window.gwire.setSchemeZoom = (editor, zoom) => {
        editor.gwireSchemePan?.setZoom(zoom);
    };

    window.gwire.disableSchemePan = (editor) => {
        editor.gwireSchemePan?.dispose();
        delete editor.gwireSchemePan;
    };
})();
