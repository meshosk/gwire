window.gwire = window.gwire || {};

window.gwire.fitSvgBackground = async (element, source, width, height, rotation) => {
    // A later parameter update must win if image decoding overlaps another render.
    const layout = {};
    element.gwireSvgBackgroundLayout = layout;
    const image = new Image();
    image.src = source;
    try {
        await image.decode();
    } catch {
        return;
    }
    if (!element.isConnected || element.gwireSvgBackgroundLayout !== layout ||
        image.naturalWidth <= 0 || image.naturalHeight <= 0) {
        return;
    }

    const angle = (rotation % 360) * Math.PI / 180;
    const cosine = Math.abs(Math.cos(angle));
    const sine = Math.abs(Math.sin(angle));
    const rotatedWidth = image.naturalWidth * cosine + image.naturalHeight * sine;
    const rotatedHeight = image.naturalWidth * sine + image.naturalHeight * cosine;
    const scale = Math.max(0, Math.min(width / rotatedWidth, height / rotatedHeight));

    element.style.width = `${image.naturalWidth * scale}px`;
    element.style.height = `${image.naturalHeight * scale}px`;
    element.style.backgroundImage = `url("${source}")`;
    element.style.transform = `translate(-50%, -50%) rotate(${rotation}deg)`;
};
