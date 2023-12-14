import {ConnectPoint, CircuitPart, Movable} from "@/components/parts/common";
import {ref, watch} from "vue";

/**
 * Just cable that can connect two points
 */
export class CircleModel extends CircuitPart {

    readonly circuitPartName: string = "cable";

    private readonly _m :Movable;

    private _isPressed = ref(false);

    constructor() {
        super(["s1", "s2"]);
      //  this.pins[0].connect(this.pins[1]);

        this._m = new Movable();

        watch(() => this.isPressed.value, (n) => {

            this.disconnectAllInternalConnections();

            if (this.isPressed.value) {
                this.pins[0].connect(this.pins[1]);
            }
            this.onPartChangeState(this);
        })
    }


    get m(): Movable {
        return this._m;
    }

    get isPressed(){

        return this._isPressed;
    }

}