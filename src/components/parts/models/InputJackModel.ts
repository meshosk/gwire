import {CircuitPart, Movable} from "@/components/parts/common";

export class InputJackModel extends CircuitPart {

    readonly circuitPartName = "Input jack";

    constructor() {
        super(["hot", "ground"]);
        this.makeAllPinsFollow(this.m);
    }

    public get hotPin(){
        return this.pinByName("hot");
    }

    public get groundPin() {
        return this.pinByName("ground")
    }
    public get JSONObject() {
        return {
            id : this.id,
            m: this.m.JSONObject,
            type: this.nonMinifiedClassName,
            hotPin : this.hotPin?.JSONObject,
            groundPin : this.groundPin?.JSONObject
        };
    }

    public setFromJSON(o :any) {
        if(this.nonMinifiedClassName != o.type) {
            throw new Error("Wrong JSON object");
        }
        this.loadMovablesFormJSON(o);
    }

    get vueComponentName(): string {
        return "InputJackView";
    }

    get nonMinifiedClassName(): string {
        return "InputJackModel";
    }
}