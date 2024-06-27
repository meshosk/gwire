import {ConnectPoint, CircuitPart, Movable, Draggable} from "@/components/parts/common";
import {Color} from "@/components/parts/common/color/Color";
import {ref} from "vue";
import * as Vue from "@vue/reactivity";

/**
 * Just cable that can connect two points
 */
export class CableModel extends CircuitPart {

    readonly circuitPartName: string = "cable";

    private _color :Color = new Color("#000000");


    private _splits : Vue.Ref<Movable[]>;

    public drawPriority : boolean = true;
    constructor() {
        super(["c1","c2"]);
        this._splits = ref([]);
        this.pins[0].connect(this.pins[1]);
    }

    get splits(): Vue.Ref<Movable[]> {
        return this._splits;
    }

    get polyLinePointsString() {

        ;
        return this.c1.draggable.XYString
            + " " + this.splits.value.map( x => x.XYString ).join(" ")
            + " " + this.c2.draggable.XYString;
    }

    override initPosition(x: number, y: number) {
        this.c1.draggable.x.value = x - 100;
        this.c1.draggable.y.value = y;
        this.c2.draggable.x.value = x + 100;
        this.c2.draggable.y.value = y;
    }

    get color(): string {
        return this._color.color;
    }

    set color(value: string) {
        this._color = new Color(value);
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
            color: this.color,
            splits : Vue.toRaw(this._splits.value).map(x => x.JSONObject)
        };
    }

    public setFromJSON(o :any) {
        if(this.nonMinifiedClassName != o.type) {
            throw new Error("Wrong JSON object");
        }
        this.color = o.color;

        let sp = o.splits.map((x :any)  => {
            let m = new Movable();
            m.setFromJSON(x);
            return m;
        })
        sp.forEach((x: Movable) => this._splits.value.push(x));

        this.loadMovablesFormJSON(o);
    }

    get vueComponentName(): string {
        return "CableView";
    }

    get nonMinifiedClassName(): string {
        return "CableModel";
    }

    public addSplit(x :number, y :number) {
        let newM = new Movable();
        newM.x.value = x;
        newM.y.value = y;

        this._splits.value.push(newM);
    }
}