/**
 * Represents a connect point (pin) definition in a component template.
 * Positions are relative to the component's origin (0,0 is top-left of the SVG viewBox).
 */
export class TemplateConnectPoint {
    /** Unique identifier for this pin, used in connections */
    id: string;
    
    /** Human-readable label for display */
    label: string;
    
    /** X position relative to component origin */
    x: number;
    
    /** Y position relative to component origin */
    y: number;

    constructor(id: string = "", label: string = "", x: number = 0, y: number = 0) {
        this.id = id;
        this.label = label;
        this.x = x;
        this.y = y;
    }

    /**
     * Create instance from JSON object
     */
    static fromJSON(o: any): TemplateConnectPoint {
        const point = new TemplateConnectPoint();
        point.id = o.id || "";
        point.label = o.label || o.id || "";
        point.x = o.x || 0;
        point.y = o.y || 0;
        return point;
    }

    /**
     * Convert to JSON object
     */
    toJSON(): object {
        return {
            id: this.id,
            label: this.label,
            x: this.x,
            y: this.y
        };
    }
}