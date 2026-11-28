export async function show(element) {
    const modal = bootstrap.Modal.getOrCreateInstance(element, { backdrop: "static", keyboard: false });
    modal.show();
    // Paint the dialog before starting repository requests.
    await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
}

export function dispose(element) {
    const modal = bootstrap.Modal.getInstance(element);
    if (modal) {
        modal.hide();
        modal.dispose();
    }
}
