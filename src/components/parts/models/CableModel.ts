import {ConnectPoint, CircuitPart} from "@/components/parts/common";

/**
 * Just cable that can connect two points
 */
export class CableModel extends CircuitPart {

    readonly circuitPartName: string = "cable";

    constructor() {
        super(2);
        this.pins[0].connect(this.pins[1]);
    }

}