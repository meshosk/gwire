/**
 * Represents a single state of a component.
 * Connections is an array of groups, where each group contains pin ids that are connected together.
 */
export class ComponentState {
    /** Unique identifier for this state */
    id: string;
    
    /** Array of connection groups. Each group is an array of pin ids that are connected together. */
    connections: string[][];

    constructor(id: string = "", connections: string[][] = []) {
        this.id = id;
        this.connections = connections;
    }

    /**
     * Create instance from JSON object
     */
    static fromJSON(o: any): ComponentState {
        const state = new ComponentState();
        state.id = o.id || "";
        state.connections = o.connections || [];
        return state;
    }

    /**
     * Convert to JSON object
     */
    toJSON(): object {
        return {
            id: this.id,
            connections: this.connections
        };
    }
}