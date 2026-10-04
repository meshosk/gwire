window.gwire = window.gwire || {};

window.gwire.enablePointerCapture = (element) => {
    element.addEventListener("pointerdown", (event) => {
        if (event.button === 0 && event.isPrimary) {
            element.setPointerCapture(event.pointerId);
        }
    });
};
