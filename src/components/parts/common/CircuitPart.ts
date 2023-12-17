import {ConnectPoint, Movable} from "@/components/parts/common";
import {ref, watch} from "vue";


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

    private _highlight = ref(HighlightType.NONE);

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

    public pinByName(name :string) : ConnectPoint|undefined {
        return this.pins.find( x =>x.name == name);
    }

    public disconnectAllInternalConnections() {
        let inner = this._pins.filter(x => x.part == this);
        for (let c of inner) {
           for (let cc  of inner) {
               c.disconnect(cc);
           }
        }
    }

    get highlight() {
        return this._highlight;
    }

    set highlight(value) {
        this._highlight = value;
    }

    public makeAllPinsFollow(m :Movable) {
        watch( () => [m.x.value, m.y.value],
            () => {
                for (let pin of this._pins) {
                    pin.draggable.x.value = m.x.value;
                    pin.draggable.y.value = m.y.value;
                }
            });
    }

}