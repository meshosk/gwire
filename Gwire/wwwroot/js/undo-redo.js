window.gwire = window.gwire || {};

window.gwire.enableUndoRedoShortcuts = (element, receiver) => {
    const focusCanvas = (event) => {
        if (event.target.closest(".scheme-editor-canvas")) {
            element.focus({ preventScroll: true });
        }
    };
    const handleKeyDown = (event) => {
        if (event.target.closest("input, textarea, select, [contenteditable]")) {
            return;
        }
        if (!(event.ctrlKey || event.metaKey) || event.altKey) {
            return;
        }
        const key = event.key.toLowerCase();
        if (key !== "z" && key !== "y") {
            return;
        }
        event.preventDefault();
        receiver.invokeMethodAsync("HandleHistoryShortcut", key === "y" || event.shiftKey);
    };
    element.addEventListener("pointerdown", focusCanvas);
    element.addEventListener("keydown", handleKeyDown);
    element.gwireUndoRedo = { focusCanvas, handleKeyDown };
};

window.gwire.disableUndoRedoShortcuts = (element) => {
    const handlers = element?.gwireUndoRedo;
    if (handlers) {
        element.removeEventListener("pointerdown", handlers.focusCanvas);
        element.removeEventListener("keydown", handlers.handleKeyDown);
        delete element.gwireUndoRedo;
    }
};
