import {ConnectPoint, CircuitPart, Movable, DraggableOver} from "@/components/parts/common";
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
        this._m = new Movable();

        this.makeAllPinsFollow(this._m);

        watch(() => this.isPressed.value, (n) => {

            this.disconnectAllInternalConnections();

            if (this.isPressed.value) {
                this.pins[0].connect(this.pins[1]);
            }
            this.onPartChangeState(this);
        })
    }

    get s1(): ConnectPoint {
        return this.pinByName("s1");
    }

    get s2(): ConnectPoint {
        return this.pinByName("s2");
    }

    get m(): Movable {
        return this._m;
    }

    get isPressed(){
        return this._isPressed;
    }

    public get JSONObject() {
        return {
            id : this.constructor.name + "_" + this.id,
            movable: this.m.JSONObject,
            type: this.constructor.name,
            isPressed: this.isPressed.value,
            s1 : this.s1.JSONObject,
            s2 : this.s2.JSONObject
        };
    }

}