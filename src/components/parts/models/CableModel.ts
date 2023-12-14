import {ConnectPoint, CircuitPart, Movable} from "@/components/parts/common";

/**
 * Just cable that can connect two points
 */
export class CableModel extends CircuitPart {

    readonly circuitPartName: string = "cable";
    private readonly _c1 :Movable;
    private readonly _c2 :Movable;

    public drawPriority : boolean = true;
    constructor() {
        super(["c1","c2"]);
        this.pins[0].connect(this.pins[1]);
        this._c1 = new Movable();
        this._c2 = new Movable();
    }

    get c1(): Movable {
        return this._c1;
    }

    get c2(): Movable {
        return this._c2;
    }
}