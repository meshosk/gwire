window.gwire = window.gwire || {};

window.gwire.enableSchemePan = (editor) => {
    const svg = editor.querySelector(".scheme-editor-canvas");
    const toggle = editor.querySelector("[data-scheme-pan-toggle]");
    const icon = toggle.querySelector("i");
    const grid = svg.querySelector('rect[fill="url(#scheme-grid)"]');
    const controller = new AbortController();
    const options = { signal: controller.signal };
    const captureOptions = { ...options, capture: true };
    let panEnabled = false;
    let spaceHeld = false;
    let pointerId = null;
    let lastX = 0;
    let lastY = 0;
    let x = 0;
    let y = 0;
    let suppressClick = false;

    const updateView = () => {
        svg.setAttribute("viewBox", `${x} ${y} ${svg.clientWidth} ${svg.clientHeight}`);
        grid.setAttribute("x", x);
        grid.setAttribute("y", y);
    };
    // Keep one scheme unit equal to one CSS pixel, including after resizing.
    svg.setAttribute("preserveAspectRatio", "none");
    const resizeObserver = new ResizeObserver(updateView);
    resizeObserver.observe(svg);
    updateView();

    const isPanMode = () => panEnabled !== spaceHeld;
    const updateMode = () => {
        const panning = isPanMode();
        svg.classList.toggle("pan-ready", panning);
        svg.classList.toggle("panning", panning && pointerId !== null);
        toggle.classList.toggle("active", panning);
        toggle.disabled = spaceHeld;
        toggle.setAttribute("aria-pressed", String(panning));
        toggle.title = panning ? "Pan mode (hold Space to edit)" : "Edit mode (hold Space to pan)";
        icon.classList.toggle("fa-computer-mouse", !panning);
        icon.classList.toggle("fa-up-down-left-right", panning);
    };
    toggle.addEventListener("click", () => {
        if (!spaceHeld) {
            panEnabled = !panEnabled;
            updateMode();
        }
    }, options);
    updateMode();

    const releaseSpace = () => {
        spaceHeld = false;
        updateMode();
    };
    window.addEventListener("keydown", (event) => {
        if (event.code !== "Space" || event.ctrlKey || event.metaKey || event.altKey ||
            event.target.closest("input, textarea, select, [contenteditable]") ||
            (event.target.closest("button, a") && event.target.closest("[data-scheme-pan-toggle]") !== toggle &&
             !svg.matches(":hover"))) {
            return;
        }
        if (editor.contains(document.activeElement) || svg.matches(":hover")) {
            event.preventDefault();
        }
        spaceHeld = true;
        updateMode();
    }, options);
    window.addEventListener("keyup", (event) => {
        if (event.code === "Space") {
            releaseSpace();
        }
    }, options);
    window.addEventListener("blur", releaseSpace, options);

    svg.addEventListener("pointerdown", (event) => {
        suppressClick = false;
        if (!isPanMode() || event.button !== 0 || !event.isPrimary || pointerId !== null) {
            return;
        }
        pointerId = event.pointerId;
        lastX = event.clientX;
        lastY = event.clientY;
        suppressClick = true;
        editor.focus({ preventScroll: true });
        svg.setPointerCapture(pointerId);
        svg.classList.add("panning");
        event.preventDefault();
        event.stopImmediatePropagation();
    }, captureOptions);
    svg.addEventListener("pointermove", (event) => {
        if (event.pointerId !== pointerId) {
            return;
        }
        if (isPanMode()) {
            x -= event.clientX - lastX;
            y -= event.clientY - lastY;
            updateView();
        }
        lastX = event.clientX;
        lastY = event.clientY;
        event.preventDefault();
        event.stopImmediatePropagation();
    }, captureOptions);
    const endPan = (event) => {
        if (event.pointerId !== pointerId) {
            return;
        }
        pointerId = null;
        svg.classList.remove("panning");
        if (svg.hasPointerCapture(event.pointerId)) {
            svg.releasePointerCapture(event.pointerId);
        }
        event.stopImmediatePropagation();
    };
    svg.addEventListener("pointerup", endPan, captureOptions);
    svg.addEventListener("pointercancel", endPan, captureOptions);
    svg.addEventListener("lostpointercapture", endPan, captureOptions);
    svg.addEventListener("click", (event) => {
        if (suppressClick) {
            suppressClick = false;
            event.preventDefault();
            event.stopImmediatePropagation();
        }
    }, captureOptions);

    editor.gwireSchemePan = () => {
        controller.abort();
        resizeObserver.disconnect();
        if (pointerId !== null && svg.hasPointerCapture(pointerId)) {
            svg.releasePointerCapture(pointerId);
        }
        releaseSpace();
    };
};

window.gwire.disableSchemePan = (editor) => {
    editor.gwireSchemePan?.();
    delete editor.gwireSchemePan;
};
