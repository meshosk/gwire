import {ConnectPoint, CircuitPart} from "@/components/parts/common";

/**
 * Just cable that can connect two points
 */
export class CircleModel extends CircuitPart {

    readonly circuitPartName: string = "cable";

    private _isPressed = false;

    constructor() {
        super(2);
        this.pins[0].connect(this.pins[1]);
    }


    get isPressed(): boolean {
        return this._isPressed;
    }

    set isPressed(value: boolean) {
        this._isPressed = value;

        this.disconnectAllInternalConnections();

        this.onPartChangeState(this);
    }
}