export class DynamicComponentState {
    id: string;
    svg?: string;
    connectionGroups: string[][];

    constructor(id: string = "", svg: string = "", connectionGroups: string[][] = []) {
        this.id = id;
        this.svg = svg || "";
        this.connectionGroups = connectionGroups;
    }

    static fromJSON(o: any): DynamicComponentState {
        const state = new DynamicComponentState();
        state.id = o.id || "";
        state.svg = o.svg || "";
        state.connectionGroups = o.connectionGroups || [];
        return state;
    }

    toJSON(): object {
        return {
            id: this.id,
            svg: this.svg,
            connectionGroups: this.connectionGroups
        };
    }
}
