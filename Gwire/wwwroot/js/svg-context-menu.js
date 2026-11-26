window.gwire = window.gwire || {};

(() => {
    /** Opens the .NET context menu on right-click and returns focus when it closes. */
    class SvgContextMenu {
        returnFocus = null;

        constructor(target, receiver) {
            this.target = target;
            this.receiver = receiver;
            target.addEventListener("contextmenu", this.open);
        }

        // Arrow-function handler retains this instance when called by DOM events.
        open = (event) => {
            if (event.defaultPrevented) {
                return;
            }
            event.preventDefault();
            this.returnFocus = document.activeElement;
            const position = new DOMPoint(event.clientX, event.clientY).matrixTransform(this.target.getScreenCTM().inverse());
            this.receiver.invokeMethodAsync("Open", event.clientX, event.clientY, position.x, position.y);
        };

        detach() {
            this.target.removeEventListener("contextmenu", this.open);
        }

        restoreFocus() {
            if (this.returnFocus?.isConnected) {
                this.returnFocus.focus({ preventScroll: true });
            }
        }
    }

    window.gwire.attachSvgContextMenu = (selector, receiver) =>
        new SvgContextMenu(document.querySelector(selector), receiver);
})();

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

window.gwire.showSvgContextSubmenu = (element) => {
    const overlay = element.closest(".svg-context-menu-overlay");
    const menu = element.querySelector('[role="menu"]');
    const bounds = overlay.getBoundingClientRect();
    const anchor = element.getBoundingClientRect();
    const scaleX = bounds.width / overlay.clientWidth;
    const scaleY = bounds.height / overlay.clientHeight;
    const anchorLeft = (anchor.left - bounds.left) / scaleX;
    const anchorTop = (anchor.top - bounds.top) / scaleY;
    menu.style.maxWidth = `${overlay.clientWidth}px`;
    menu.style.maxHeight = `${overlay.clientHeight}px`;
    const right = anchorLeft + element.offsetWidth;
    const left = right + menu.offsetWidth <= overlay.clientWidth ? right : anchorLeft - menu.offsetWidth;
    menu.style.left = `${Math.max(0, Math.min(left, overlay.clientWidth - menu.offsetWidth)) - anchorLeft}px`;
    menu.style.top = `${Math.max(0, Math.min(anchorTop, overlay.clientHeight - menu.offsetHeight)) - anchorTop}px`;
    menu.querySelector('.dropdown-item:not(:disabled)')?.focus({ preventScroll: true });
};
