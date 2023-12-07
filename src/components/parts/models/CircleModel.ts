import {ConnectPoint, CircuitPart} from "@/components/parts/common";
import { ref, watch} from "vue";

/**
 * Just cable that can connect two points
 */
export class CircleModel extends CircuitPart {

    readonly circuitPartName: string = "cable";

    private readonly _isPressed = ref(false);

    constructor() {
        super(["c1", "c2"]);
        this.pins[0].connect(this.pins[1]);

        watch(() => this.isPressed.value, (n) => {
            this.disconnectAllInternalConnections();
            this.onPartChangeState(this);
        })

    }

    get isPressed(){
        return this._isPressed;
    }
}