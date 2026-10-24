import { show as showModal, hide, dispose } from "../../js/confirmation-modal.js";

export { hide, dispose };

export function show(element, receiver) {
    element.addEventListener("shown.bs.modal", () => {
        element.querySelector("input")?.focus({ preventScroll: true });
    }, { once: true });
    showModal(element, receiver);
}
