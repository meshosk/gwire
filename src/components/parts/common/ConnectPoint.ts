import {CircuitPart, DraggableOver, Movable} from "@/components/parts/common";

/**
 * Simulates internal connections of a single part
 * One connect can connect multiple connectors. Connections are two-way
 */
export class ConnectPoint {

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

    public get asJSONObject() {


    }

}