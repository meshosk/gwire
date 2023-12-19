import {CircuitPart, Movable} from "@/components/parts/common";

export class InputJackModel extends CircuitPart {

    readonly circuitPartName = "Input jack";

    private _m :Movable =  new Movable();
    constructor() {
        super(["hot", "ground"]);
        this.makeAllPinsFollow(this._m);
    }

    public get hotPin(){
        return this.pinByName("hot");
    }

    public get groundPin() {
        return this.pinByName("ground")
    }
    get m(): Movable {
        return this._m;
    }

    public get JSONObject() {
        return {
            id : this.id,
            m: this.m.JSONObject,
            type: this.constructor.name,
            hotPin : this.hotPin?.JSONObject,
            groundPin : this.groundPin?.JSONObject
        };
    }

    public setFromJSON(o :any) {
        if(this.constructor.name != o.type) {
            throw new Error("Wrong JSON object");
        }
        this.loadMovablesFormJSON(o);
    }

    get vueComponentName(): string {
        return "InputJackView";
    }
}