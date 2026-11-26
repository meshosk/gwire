import { ConfirmationModal } from "./confirmation-modal.js";
export { hide, dispose } from "./confirmation-modal.js";

/** Confirmation modal that focuses and selects its text input once shown. */
class PartTextModal extends ConfirmationModal {
    show() {
        this.element.addEventListener("shown.bs.modal", this.selectInput, { once: true });
        super.show();
    }

    selectInput = () => {
        const input = this.element.querySelector("input");
        input.focus({ preventScroll: true });
        input.select();
    };
}

export function show(element, receiver) {
    new PartTextModal(element, receiver).show();
}
