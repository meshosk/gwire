window.gwire = window.gwire || {};

window.gwire.enablePointerCapture = (element) => {
    element.setAttribute("data-pointer-capture", "");
    element.addEventListener("pointerdown", (event) => {
        if (event.button === 0 && event.isPrimary &&
            event.target.closest("[data-pointer-capture]") === element) {
            element.setPointerCapture(event.pointerId);
        }
    });
};

window.gwire.getSvgPoint = (element, clientX, clientY) => {
    const position = new DOMPoint(clientX, clientY).matrixTransform(element.getScreenCTM().inverse());
    return { x: position.x, y: position.y };
};

window.gwire.focusSchemeEditor = (element) => {
    element.closest(".scheme-editor")?.focus({ preventScroll: true });
};
