import { ComponentState } from "./ComponentState";
import { TemplateConnectPoint } from "./TemplateConnectPoint";

/**
 * Represents a component template that defines:
 * - SVG graphics
 * - Connect points (pins) with positions
 * - States with internal connections
 */
export class ComponentTemplate {
    /** Unique name identifier for the template */
    name: string;
    
    /** Display name shown in UI */
    displayName: string;
    
    /** SVG string for the component graphics */
    svg: string;
    
    /** Array of connect point definitions */
    pins: TemplateConnectPoint[];
    
    /** Array of states defining different connection configurations */
    states: ComponentState[];
    
    /** Default state id to use when component is first created */
    defaultState: string;

    constructor(name: string = "", displayName: string = "", svg: string = "", 
                pins: TemplateConnectPoint[] = [], states: ComponentState[] = [], 
                defaultState: string = "") {
        this.name = name;
        this.displayName = displayName || name;
        this.svg = svg;
        this.pins = pins;
        this.states = states;
        this.defaultState = defaultState || (states[0]?.id || "");
    }

    /**
     * Create instance from JSON object
     */
    static fromJSON(o: any): ComponentTemplate {
        const template = new ComponentTemplate();
        template.name = o.name || "";
        template.displayName = o.displayName || o.name || "";
        template.svg = o.svg || "";
        template.pins = (o.pins || []).map((p: any) => TemplateConnectPoint.fromJSON(p));
        template.states = (o.states || []).map((s: any) => ComponentState.fromJSON(s));
        template.defaultState = o.defaultState || (o.states?.[0]?.id || "");
        return template;
    }

    /**
     * Convert to JSON object
     */
    toJSON(): object {
        return {
            name: this.name,
            displayName: this.displayName,
            svg: this.svg,
            pins: this.pins.map(p => p.toJSON()),
            states: this.states.map(s => s.toJSON()),
            defaultState: this.defaultState
        };
    }

    /**
     * Get pin by id
     */
    getPinById(id: string): TemplateConnectPoint | undefined {
        return this.pins.find(p => p.id === id);
    }

    /**
     * Get state by id
     */
    getStateById(id: string): ComponentState | undefined {
        return this.states.find(s => s.id === id);
    }

    /**
     * Validate the template has all required data
     */
    isValid(): boolean {
        return this.name.length > 0 && 
               this.svg.length > 0 && 
               this.pins.length > 0 && 
               this.states.length > 0;
    }
}