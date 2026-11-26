/** Shows a Bootstrap modal and reports its result to .NET after it has fully closed. */
export class ConfirmationModal {
    result = 0;

    constructor(element, receiver) {
        this.element = element;
        this.receiver = receiver;
    }

    show() {
        const element = this.element;
        this.modal = bootstrap.Modal.getOrCreateInstance(element);
        this.previousFocus = document.activeElement;
        this.shown = new Promise(resolve => {
            element.addEventListener("shown.bs.modal", resolve, { once: true });
        });
        this.hidden = new Promise(resolve => {
            element.addEventListener("hidden.bs.modal", resolve, { once: true });
        });
        element.gwireConfirmation = this;
        element.addEventListener("hidden.bs.modal", this.handleHidden, { once: true });
        this.modal.show();
    }

    // Arrow-function handler retains this instance when called by DOM events.
    handleHidden = async () => {
        if (this.previousFocus?.isConnected) {
            this.previousFocus.focus({ preventScroll: true });
        }
        await this.receiver.invokeMethodAsync("Complete", this.result);
    };

    async hide(result) {
        await this.shown;
        this.result = result;
        this.modal.hide();
        await this.hidden;
    }

    async dispose() {
        const element = this.element;
        element.removeEventListener("hidden.bs.modal", this.handleHidden);
        await this.shown;
        if (element.classList.contains("show")) {
            this.modal.hide();
            await this.hidden;
        }
        await this.hidden;
        this.modal.dispose();
        delete element.gwireConfirmation;
    }
}

export function show(element, receiver) {
    new ConfirmationModal(element, receiver).show();
}

export async function hide(element, result) {
    await element.gwireConfirmation?.hide(result);
}

export async function dispose(element) {
    await element.gwireConfirmation?.dispose();
}
