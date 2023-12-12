import {ConnectPoint} from "@/components/parts/common";
import {ref} from "vue";


export  enum HighlightType {
    NONE,
    SELECTED,
    ROUTE
}

export abstract class CircuitPart {

    private static  idCount = 0;
    private static getId() {
        return CircuitPart.idCount++;
    }


    private readonly _id;
    private readonly _pins :ConnectPoint[] = [];

    private _highlight;

    public drawPriority : boolean = false;

    public onPartChangeState :(circlePart: CircuitPart) => void = (circlePart) =>{};

    abstract readonly circuitPartName :string;

    public get vueComponentName(): string {
        return this.constructor.name.replace("Model", "View");
    }

    protected constructor(pinsNames : string[]|number[]) {
        this._id = CircuitPart.getId();
        this._highlight = ref(HighlightType.NONE);
        for (let i = 0; i < pinsNames.length; i++) {
            this._pins.push(new ConnectPoint(this, pinsNames[i]));
        }
    }

    get id() {
        return this._id;
    }

    public get pins(): ConnectPoint[] {
        return this._pins;
    }

    public disconnectAllInternalConnections() {
        for (let c of this._pins) {
            c.disconnectInternalConnectionsOnly();
        }
    }

    get highlight() {
        return this._highlight;
    }

    set highlight(value) {
        this._highlight = value;
    }
}