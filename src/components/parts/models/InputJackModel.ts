import {CircuitPart, Movable} from "@/components/parts/common";

export class InputJackModel extends CircuitPart {

    readonly circuitPartName: "Input jack";

    private _m :Movable =  new Movable();
    constructor() {
        super(["hot", "ground"]);
    }

    public get signalPin(){
        return this.pins[0];
    }

    public get groundPin() {
        return this.pins[1];
    }


    get m(): Movable {
        return this._m;
    }
}