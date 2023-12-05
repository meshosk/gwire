import {ConnectPoint, CircuitPart} from "@/components/parts";

/**
 * Just cable that can connect two points
 */
export class Cable extends CircuitPart {

    readonly circuitPartName: string = "cable";
    public readonly vueComponentName: string = "Cable";

    constructor() {
        super(2);
     //   this.pins[0].connect(this.pins[1]);
    }



}