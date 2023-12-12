import {ConnectPoint, CircuitPart} from "@/components/parts/common";
import { ref, watch} from "vue";

/**
 * Just cable that can connect two points
 */
export class CircleModel extends CircuitPart {

    readonly circuitPartName: string = "cable";

    private readonly _isPressed = ref(false);

    constructor() {
        super(["s1", "s2"]);
      //  this.pins[0].connect(this.pins[1]);

        watch(() => this.isPressed.value, (n) => {

            this.disconnectAllInternalConnections();

            if (this.isPressed.value) {
                this.pins[0].connect(this.pins[1]);
            }
            this.onPartChangeState(this);
        })

    }

    get isPressed(){

        return this._isPressed;
    }
}