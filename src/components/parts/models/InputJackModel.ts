import {CircuitPart} from "@/components/parts/common";

export class InputJackModel extends CircuitPart {

    readonly circuitPartName: "Input jack";

    constructor() {
        super(["hot", "ground"]);
    }

    public get signalPin(){
        return this.pins[0];
    }

    public get groundPin() {
        return this.pins[1];
    }
}