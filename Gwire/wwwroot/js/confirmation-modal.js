export function show(element, receiver) {
    const modal = bootstrap.Modal.getOrCreateInstance(element);
    const previousFocus = document.activeElement;
    const shown = new Promise(resolve => {
        element.addEventListener("shown.bs.modal", resolve, { once: true });
    });
    const hidden = new Promise(resolve => {
        element.addEventListener("hidden.bs.modal", resolve, { once: true });
    });
    const state = { modal, shown, hidden, result: 0 };
    state.handleHidden = async () => {
        if (previousFocus?.isConnected) {
            previousFocus.focus({ preventScroll: true });
        }
        await receiver.invokeMethodAsync("Complete", state.result);
    };
    element.gwireConfirmation = state;
    element.addEventListener("hidden.bs.modal", state.handleHidden, { once: true });
    modal.show();
}

export async function hide(element, result) {
    const state = element.gwireConfirmation;
    if (state) {
        await state.shown;
        state.result = result;
        state.modal.hide();
        await state.hidden;
    }
}

export async function dispose(element) {
    const state = element.gwireConfirmation;
    if (!state) {
        return;
    }
    element.removeEventListener("hidden.bs.modal", state.handleHidden);
    await state.shown;
    if (element.classList.contains("show")) {
        state.modal.hide();
        await state.hidden;
    }
    await state.hidden;
    state.modal.dispose();
    delete element.gwireConfirmation;
}
