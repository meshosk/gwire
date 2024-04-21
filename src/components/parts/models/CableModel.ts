import {ConnectPoint, CircuitPart, Movable} from "@/components/parts/common";

/**
 * Just cable that can connect two points
 */
export class CableModel extends CircuitPart {

    readonly circuitPartName: string = "cable";

    private _color :string = "#000000";

    public drawPriority : boolean = true;
    constructor() {
        super(["c1","c2"]);



        this.pins[0].connect(this.pins[1]);
    }

    override initPosition(x: number, y: number) {
        this.c1.draggable.x.value = x - 100;
        this.c1.draggable.y.value = y;
        this.c2.draggable.x.value = x + 100;
        this.c2.draggable.y.value = y;
    }


    get color(): string {
        return this._color;
    }

    set color(value: string) {
        this._color = value;
    }

    get c1(): ConnectPoint {
        return this.pins[0];
    }

    get c2(): ConnectPoint {
        return this.pins[1];
    }

    public get JSONObject() {
        return {
            id : this.id,
            type: this.nonMinifiedClassName,
            c1 : this.c1.JSONObject,
            c2 : this.c2.JSONObject,
            color: this.color
        };
    }

    public setFromJSON(o :any) {
        if(this.nonMinifiedClassName != o.type) {
            throw new Error("Wrong JSON object");
        }

        this.color = o.color;

        this.loadMovablesFormJSON(o);
    }

    get vueComponentName(): string {
        return "CableView";
    }

    get nonMinifiedClassName(): string {
        return "CableModel";
    }
}