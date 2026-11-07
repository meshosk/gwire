import { show as showModal, hide, dispose } from "../../../js/confirmation-modal.js";

export { hide, dispose };

export function show(element, receiver) {
    const handleKeyDown = async event => {
        const result = event.target.closest("[data-search-result-index]");
        if (event.key !== "ArrowDown" && event.key !== "ArrowUp" && event.key !== "Escape" &&
            !(event.key === "Enter" && result)) {
            return;
        }

        // Intercept navigation before Bootstrap handles Escape or Blazor handles the input's Enter key.
        event.preventDefault();
        event.stopImmediatePropagation();
        await receiver.invokeMethodAsync("HandleNavigationKey", event.key,
            result ? Number(result.dataset.searchResultIndex) : null);
    };
    element.addEventListener("keydown", handleKeyDown, true);
    element.addEventListener("hidden.bs.modal", () => {
        element.removeEventListener("keydown", handleKeyDown, true);
    }, { once: true });
    element.addEventListener("shown.bs.modal", () => {
        focus(element, null);
    }, { once: true });
    showModal(element, receiver);
}

export function focus(element, resultIndex) {
    const target = resultIndex === null
        ? element.querySelector("input")
        : element.querySelector(`[data-search-result-index="${resultIndex}"]`);
    target?.focus({ preventScroll: true });
    if (resultIndex !== null) {
        target?.scrollIntoView({ block: "nearest" });
    }
}
