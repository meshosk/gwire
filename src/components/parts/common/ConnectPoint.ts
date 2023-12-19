import {CircuitPart, DraggableOver, HighlightType, Movable} from "@/components/parts/common";
import {ref} from "vue";

/**
 * Simulates internal connections of a single part
 * One connect can connect multiple connectors. Connections are two-way
 */
export class ConnectPoint {

    private _highlight = ref(HighlightType.NONE);
    private readonly _part : CircuitPart;
    private _connectedTo :ConnectPoint[] = [];
    private readonly _name: string;

    private readonly _draggable :DraggableOver = new DraggableOver(this);

    constructor(part: CircuitPart, name :string) {
        this._part = part;
        this._name = name;
    }

    get name(): string {
        return this._name;
    }

    get part(): CircuitPart {
        return this._part;
    }

    public connect(connectPoint : ConnectPoint) {
        if (this.isConnectedTo(connectPoint) == false) {
            this._connectedTo.push(connectPoint);
            connectPoint.connect(this);
        }
    }

    public  disconnect(connectPoint : ConnectPoint) {
        if (this.isConnectedTo(connectPoint) == true) {
            let index = this._connectedTo.indexOf(connectPoint);
            this._connectedTo.splice(index, 1);
            connectPoint.disconnect(this);
        }
    }

    isConnectedTo(connectPoint : ConnectPoint) : boolean {
        return this._connectedTo.indexOf(connectPoint) > -1;
    }
    /**
     * Return copy of connected items
     */
    get connectedTo(): ConnectPoint[] {
        // make copy of connected items
        return this._connectedTo;
    }


    get draggable(): DraggableOver {
        return this._draggable;
    }


    get highlight() {
        return this._highlight;
    }

    set highlight(value) {
        this._highlight = value;
    }


    public get JSONObject() {
        let o = this.baseJSONObject;
        // @ts-ignore
        o.connectedTo = [];
        for (let cTo of this._connectedTo) {
            if (cTo.part != this.part) {
                // @ts-ignore
                o.connectedTo.push(cTo.baseJSONObject)
            }
        }
        return o;
    }

    private get baseJSONObject() {
        return {
            name: this.name,
            part: this.part.id,
            draggable: this.draggable.JSONObject,
        }
    }

    public setFromJSON(o :any) {
        if(this.name != o.name) {
            throw Error("Wrong JSON object");
        }
        this.draggable.setFromJSON(o.draggable);
    }

}