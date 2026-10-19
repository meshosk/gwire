import { show as showModal } from "./confirmation-modal.js";
export { hide, dispose } from "./confirmation-modal.js";

export function show(element, receiver) {
    element.addEventListener("shown.bs.modal", () => {
        const input = element.querySelector("input");
        input.focus({ preventScroll: true });
        input.select();
    }, { once: true });
    showModal(element, receiver);
}
