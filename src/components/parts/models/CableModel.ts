import {ConnectPoint, CircuitPart} from "@/components/parts/common";

/**
 * Just cable that can connect two points
 */
export class CableModel extends CircuitPart {

    readonly circuitPartName: string = "cable";



    public drawPriority : boolean = true;
    constructor() {
        super(["c1","c2"]);
        this.pins[0].connect(this.pins[1]);
    }



}