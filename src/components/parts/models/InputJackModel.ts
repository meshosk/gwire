import {CircuitPart, Movable} from "@/components/parts/common";

export class InputJackModel extends CircuitPart {

    readonly circuitPartName: "Input jack";

    private _m :Movable =  new Movable();
    constructor() {
        super(["hot", "ground"]);
        this.makeAllPinsFollow(this._m);
    }

    public get hotPin(){
        return this.pinByName("hot");
    }

    public get groundPin() {
        return this.pinByName("ground")
    }

    get m(): Movable {
        return this._m;
    }
}