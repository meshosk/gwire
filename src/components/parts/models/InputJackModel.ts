import {CircuitPart, Movable} from "@/components/parts/common";

export class InputJackModel extends CircuitPart {

    readonly circuitPartName: "Input jack";

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
            movable: this.m.JSONObject,
            type: this.constructor.name,
            hot : this.hotPin?.JSONObject,
            ground : this.groundPin?.JSONObject
        };
    }

    public setFromJSON(o) {
        if(this.constructor.name != o.type) {
            throw Error("Wrong JSON object");
        }
        this.m.setFromJSON(o.movable);
        this.hotPin.setFromJSON(o.hot);
        this.groundPin.setFromJSON(o.ground)
    }
}