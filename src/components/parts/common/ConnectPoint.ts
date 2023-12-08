import {CircuitPart} from "@/components/parts/common";

/**
 * Simulates internal connections of a single part
 * One connect can connect multiple connectors. Connections are two-way
 */
export class ConnectPoint {

    private readonly _part : CircuitPart;
    private _connectedTo :ConnectPoint[] = [];
    private readonly _name: string;

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

    public test() {
        return "test";
    }

    public connect(connectPoint : ConnectPoint) {
        if (this.isConnectedTo(connectPoint)) {
            this._connectedTo.push(connectPoint);
            connectPoint.connect(this);
        }
    }

    public  disconnect(connectPoint : ConnectPoint) {
        let index = this._connectedTo.indexOf(connectPoint);
        if (index >= 0) {
            let disconnected = this._connectedTo[index];
            disconnected.disconnect(connectPoint);
            this._connectedTo.slice(index, 1);
        }
    }

    disconnectMore(connections: ConnectPoint[]) {
        for (let s of connections) {
            for (let t of connections) {
                s.disconnect(t);
            }
        }
    }

    public disconnectInternalConnectionsOnly(){
        for (let c of this.connectedTo) {
            if (c.part ==  this) {
                this.disconnect(c);
            }
        }
    }

    public disconnectAll() {
        for (let c of this.connectedTo) {
            this.disconnect(c);
        }
    }

    get isConnectedToAny(): boolean {
        return this._connectedTo.length > 0;
    }

    isConnectedTo(connectPoint : ConnectPoint) : boolean {
        return this._connectedTo.indexOf(connectPoint) >= 0;
    }
    /**
     * Return copy of connected items
     */
    get connectedTo(): ConnectPoint[] {
        // make copy of connected items
        return this._connectedTo.map(x => x);
    }



}