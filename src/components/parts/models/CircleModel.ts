import {ConnectPoint, CircuitPart, Movable, Draggable} from "@/components/parts/common";
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
        return this.pins[0];
    }

    get s2(): ConnectPoint {
        return this.pins[1];
    }

    get m(): Movable {
        return this._m;
    }

    get isPressed(){
        return this._isPressed;
    }

    public get JSONObject() {
        return {
            id : this.id,
            type: this.nonMinifiedClassName,
            m: this.m.JSONObject,
            isPressed: this.isPressed.value,
            s1 : this.s1.JSONObject,
            s2 : this.s2.JSONObject
        };
    }

    public setFromJSON(o :any) {
        if(this.nonMinifiedClassName != o.type) {
            throw new Error("Wrong JSON object");
        }

        this.isPressed.value = o.isPressed;
        this.loadMovablesFormJSON(o);
    }

    get vueComponentName(): string {
        return "CircleView";
    }

    get nonMinifiedClassName(): string {
        return "CircleModel";
    }

}