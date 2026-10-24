window.gwire = window.gwire || {};

(() => {
    /** Creates a PDF from the complete scheme, independently of the editor viewport. */
    class SchemePdfExporter {
        static svgNamespace = "http://www.w3.org/2000/svg";
        static styleProperties = [
            "fill", "fill-opacity", "fill-rule", "stroke", "stroke-width", "stroke-opacity",
            "stroke-linecap", "stroke-linejoin", "stroke-miterlimit", "stroke-dasharray",
            "stroke-dashoffset", "opacity", "font-family", "font-size", "font-weight",
            "font-style", "letter-spacing", "text-anchor", "dominant-baseline", "paint-order",
            "visibility", "display", "overflow"
        ];

        static async export(editor, fileName) {
            await document.fonts.ready;
            const source = editor.querySelector("[data-pdf-content]");
            if (!source || !source.children.length) {
                throw new Error("There is no scheme to export.");
            }

            const content = source.cloneNode(true);
            const originals = [source, ...source.querySelectorAll("*")];
            const copies = [content, ...content.querySelectorAll("*")];
            let padding = 4;
            for (let index = 0; index < originals.length; index++) {
                const original = originals[index];
                const copy = copies[index];
                const style = getComputedStyle(original);
                for (const property of this.styleProperties) {
                    copy.style.setProperty(property, style.getPropertyValue(property));
                }

                // Include stroke joins as getBBox() does not include strokes in every browser.
                if (style.stroke !== "none" && !original.matches(".circuit-selection-outline, .cable-selection-outline, .cable-bend-point")) {
                    const radius = parseFloat(style.strokeWidth) / 2;
                    const join = style.strokeLinejoin === "miter" ? parseFloat(style.strokeMiterlimit) : 1;
                    padding = Math.max(padding, radius * join + 2);
                }
                if (original.matches(".connection-highlight")) {
                    copy.style.fill = "#1677d2";
                }
                if (original.matches(".cable-selected")) {
                    copy.style.stroke = "#fff";
                }
                if (original.localName === "foreignObject") {
                    this.replaceBackground(original, copy);
                }
            }
            content.querySelectorAll(".circuit-selection-outline, .cable-selection-outline, .cable-bend-point")
                .forEach(element => element.remove());

            const svg = document.createElementNS(this.svgNamespace, "svg");
            svg.append(content);
            svg.style.position = "fixed";
            svg.style.left = "-100000px";
            svg.style.top = "0";
            document.body.append(svg);
            let bounds;
            try {
                bounds = content.getBBox();
            } finally {
                svg.remove();
                svg.removeAttribute("style");
            }

            const width = bounds.width + padding * 2;
            const height = bounds.height + padding * 2;
            if (![bounds.x, bounds.y, width, height].every(Number.isFinite) || width <= 0 || height <= 0) {
                throw new Error("The scheme has invalid dimensions.");
            }
            svg.setAttribute("xmlns", this.svgNamespace);
            svg.setAttribute("viewBox", `${bounds.x - padding} ${bounds.y - padding} ${width} ${height}`);
            svg.setAttribute("width", width);
            svg.setAttribute("height", height);

            // Rasterize at 3x for crisp text and embedded SVG artwork, with bounded canvas memory.
            const scale = Math.min(3, 8192 / width, 8192 / height, Math.sqrt(16000000 / (width * height)));
            const canvas = document.createElement("canvas");
            canvas.width = Math.max(1, Math.ceil(width * scale));
            canvas.height = Math.max(1, Math.ceil(height * scale));
            const image = new Image();
            image.src = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(new XMLSerializer().serializeToString(svg))}`;
            await image.decode();
            const context = canvas.getContext("2d");
            if (!context) {
                throw new Error("Could not create the PDF image.");
            }
            context.fillStyle = "#fff";
            context.fillRect(0, 0, canvas.width, canvas.height);
            context.drawImage(image, 0, 0, canvas.width, canvas.height);

            // A custom page follows the scheme's aspect ratio, without A4 whitespace or clipping.
            const pageScale = Math.min(0.75, 14400 / Math.max(width, height));
            const pageWidth = width * pageScale;
            const pageHeight = height * pageScale;
            const pdf = new window.jspdf.jsPDF({
                orientation: pageWidth > pageHeight ? "landscape" : "portrait",
                unit: "pt",
                format: [pageWidth, pageHeight],
                compress: true
            });
            pdf.addImage(canvas.toDataURL("image/png"), "PNG", 0, 0, pageWidth, pageHeight);
            await pdf.save(fileName, { returnPromise: true });
        }

        static replaceBackground(original, copy) {
            const background = original.querySelector(".svg-background");
            const style = background && getComputedStyle(background);
            if (!style || style.backgroundImage === "none") {
                throw new Error("A part image is still loading. Please retry the export.");
            }
            const width = parseFloat(style.width);
            const height = parseFloat(style.height);
            if (!(width > 0 && height > 0)) {
                throw new Error("A part image has invalid dimensions.");
            }
            const centerX = original.width.baseVal.value / 2;
            const centerY = original.height.baseVal.value / 2;
            const transform = new DOMMatrix(style.transform);
            const angle = Math.atan2(transform.b, transform.a) * 180 / Math.PI;
            const image = document.createElementNS(this.svgNamespace, "image");
            image.setAttribute("x", centerX - width / 2);
            image.setAttribute("y", centerY - height / 2);
            image.setAttribute("width", width);
            image.setAttribute("height", height);
            image.setAttribute("transform", `rotate(${angle} ${centerX} ${centerY})`);
            const url = style.backgroundImage.slice(4, -1).replace(/^(["'])(.*)\1$/, "$2");
            image.setAttribute("href", url);
            copy.replaceWith(image);
        }
    }

    window.gwire.showPdfExportProgress = async element => {
        element.gwirePreviousFocus = document.activeElement;
        const modal = bootstrap.Modal.getOrCreateInstance(element, { backdrop: "static", keyboard: false });
        modal.show();
        element.gwireProgressObserver = new MutationObserver(() => {
            if (!element.isConnected) {
                window.gwire.hidePdfExportProgress(element);
            }
        });
        element.gwireProgressObserver.observe(document.body, { childList: true, subtree: true });
        // Give the browser time to paint the modal before PDF processing starts.
        await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
    };

    window.gwire.hidePdfExportProgress = element => {
        if (!element) {
            return;
        }
        element.gwireProgressObserver?.disconnect();
        delete element.gwireProgressObserver;
        const modal = bootstrap.Modal.getInstance(element);
        if (modal) {
            modal.hide();
            modal.dispose();
        }
        if (element.gwirePreviousFocus?.isConnected) {
            element.gwirePreviousFocus.focus({ preventScroll: true });
        }
        delete element.gwirePreviousFocus;
    };

    window.gwire.exportSchemePdf = (editor, fileName) => SchemePdfExporter.export(editor, fileName);
})();
