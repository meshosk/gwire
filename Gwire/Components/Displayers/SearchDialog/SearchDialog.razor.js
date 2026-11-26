import { ConfirmationModal, hide, dispose } from "../../../js/confirmation-modal.js";

export { hide, dispose };

/** Confirmation modal with keyboard navigation between the search input and its results. */
class SearchDialog extends ConfirmationModal {
    show() {
        const element = this.element;
        element.addEventListener("keydown", this.handleKeyDown, true);
        element.addEventListener("hidden.bs.modal", this.removeKeyDown, { once: true });
        element.addEventListener("shown.bs.modal", () => {
            SearchDialog.focus(element, null);
        }, { once: true });
        super.show();
    }

    handleKeyDown = async event => {
        const result = event.target.closest("[data-search-result-index]");
        if (event.key !== "ArrowDown" && event.key !== "ArrowUp" && event.key !== "Escape" &&
            !(event.key === "Enter" && result)) {
            return;
        }

        // Intercept navigation before Bootstrap handles Escape or Blazor handles the input's Enter key.
        event.preventDefault();
        event.stopImmediatePropagation();
        await this.receiver.invokeMethodAsync("HandleNavigationKey", event.key,
            result ? Number(result.dataset.searchResultIndex) : null);
    };

    removeKeyDown = () => {
        this.element.removeEventListener("keydown", this.handleKeyDown, true);
    };

    static focus(element, resultIndex) {
        const target = resultIndex === null
            ? element.querySelector("input")
            : element.querySelector(`[data-search-result-index="${resultIndex}"]`);
        target?.focus({ preventScroll: true });
        if (resultIndex !== null) {
            target?.scrollIntoView({ block: "nearest" });
        }
    }
}

export function show(element, receiver) {
    new SearchDialog(element, receiver).show();
}

export function focus(element, resultIndex) {
    SearchDialog.focus(element, resultIndex);
}
