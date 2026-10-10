window.gwire = window.gwire || {};

window.gwire.attachSvgContextMenu = (selector, receiver) => {
    const target = document.querySelector(selector);
    let returnFocus;
    const open = (event) => {
        if (event.defaultPrevented) {
            return;
        }
        event.preventDefault();
        returnFocus = document.activeElement;
        const position = new DOMPoint(event.clientX, event.clientY).matrixTransform(target.getScreenCTM().inverse());
        receiver.invokeMethodAsync("Open", event.clientX, event.clientY, position.x, position.y);
    };
    target.addEventListener("contextmenu", open);
    return {
        detach: () => target.removeEventListener("contextmenu", open),
        restoreFocus: () => {
            if (returnFocus?.isConnected) {
                returnFocus.focus({ preventScroll: true });
            }
        }
    };
};

window.gwire.showSvgContextMenu = (overlay, clientX, clientY) => {
    const layer = overlay.parentElement;
    const svg = layer.ownerSVGElement;
    if (svg.hasAttribute("viewBox")) {
        const view = svg.viewBox.baseVal;
        layer.setAttribute("x", view.x);
        layer.setAttribute("y", view.y);
        layer.setAttribute("width", view.width);
        layer.setAttribute("height", view.height);
    }
    const menu = overlay.querySelector('[role="menu"]');
    const bounds = overlay.getBoundingClientRect();
    const left = (clientX - bounds.left) * overlay.clientWidth / bounds.width;
    const top = (clientY - bounds.top) * overlay.clientHeight / bounds.height;
    menu.style.left = `${Math.max(0, Math.min(left, overlay.clientWidth - menu.offsetWidth))}px`;
    menu.style.top = `${Math.max(0, Math.min(top, overlay.clientHeight - menu.offsetHeight))}px`;
    const item = menu.querySelector('[role="menuitem"]:not(:disabled)');
    (item || overlay).focus({ preventScroll: true });
};
