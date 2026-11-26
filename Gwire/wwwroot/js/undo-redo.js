window.gwire = window.gwire || {};

(() => {
    /** Handles undo/redo and delete keyboard shortcuts for one scheme or part editor. */
    class UndoRedoShortcuts {
        constructor(element, receiver) {
            this.element = element;
            this.receiver = receiver;
            element.addEventListener("pointerdown", this.focusCanvas);
            element.addEventListener("keydown", this.handleKeyDown);
        }

        // Arrow-function handlers retain this instance when called by DOM events.
        focusCanvas = (event) => {
            if (event.target.closest(".scheme-editor-canvas, .editor-canvas-panel")) {
                this.element.focus({ preventScroll: true });
            }
        };

        handleKeyDown = async (event) => {
            if (event.target.closest("input, textarea, select, [contenteditable]")) {
                return;
            }
            if ((event.key === "Backspace" || event.key === "Delete") &&
                !event.ctrlKey && !event.metaKey && !event.altKey) {
                if (this.element.classList.contains("scheme-editor")) {
                    event.preventDefault();
                    await this.receiver.invokeMethodAsync("DeleteSelectedPart");
                    return;
                }
                if (this.element.classList.contains("part-editor")) {
                    event.preventDefault();
                    await this.receiver.invokeMethodAsync("DeleteSelectedPoint");
                    return;
                }
            }
            if (!(event.ctrlKey || event.metaKey) || event.altKey) {
                return;
            }
            const key = event.key.toLowerCase();
            if (key !== "z" && key !== "y") {
                return;
            }
            event.preventDefault();
            await this.receiver.invokeMethodAsync("HandleHistoryShortcut", key === "y" || event.shiftKey);
        };

        dispose() {
            this.element.removeEventListener("pointerdown", this.focusCanvas);
            this.element.removeEventListener("keydown", this.handleKeyDown);
        }
    }

    window.gwire.enableUndoRedoShortcuts = (element, receiver) => {
        element.gwireUndoRedo = new UndoRedoShortcuts(element, receiver);
    };

    window.gwire.disableUndoRedoShortcuts = (element) => {
        if (element?.gwireUndoRedo) {
            element.gwireUndoRedo.dispose();
            delete element.gwireUndoRedo;
        }
    };
})();
