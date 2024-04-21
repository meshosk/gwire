import {ConnectPoint, Movable} from "@/components/parts/common";
import {ref, watch} from "vue";
import {Serializable} from "@/components/parts/models/Serializable";


export  enum HighlightType {
    NONE,
    SELECTED,
    ROUTE
}

export abstract class CircuitPart extends Serializable {

    private static  idCount = 0;
    private static getId() {
        return CircuitPart.idCount++;
    }

    private readonly _id;
    private readonly _pins :ConnectPoint[] = [];

    private _highlight = ref(HighlightType.NONE);

    public drawPriority : boolean = false;

    /**
     * this is the main movable used when item is moved by mouse
     * @private
     */
    private readonly _m :Movable;
    /**
     * Main movable
     */
    public get m(): Movable {
        return this._m;
    }


    public onPartChangeState :(circlePart: CircuitPart) => void = (circlePart) =>{};

    abstract readonly circuitPartName :string;

    public abstract get vueComponentName(): string;
    public abstract get nonMinifiedClassName(): string;

    /**
     * Used only to set up position for first time
     * @param x
     * @param y
     */
    public initPosition(x:number,y:number): void {
        this.m.x.value = x;
        this.m.y.value = y;
    }


    protected constructor(pinsNames : string[]|number[]) {
        super();
        this._m = new Movable();
        this._id = CircuitPart.getId();
        this._highlight = ref(HighlightType.NONE);
        for (let i = 0; i < pinsNames.length; i++) {
            this._pins.push(new ConnectPoint(this, String(pinsNames[i])));
        }
    }

    get id() {
        return this.nonMinifiedClassName + "_" +this._id;
    }

    public get pins(): ConnectPoint[] {
        return this._pins;
    }

    public pinByName(name :string) : ConnectPoint|undefined {
        return this.pins.find( x =>x.name == name);
    }

    public get internalPins() {
        return this._pins.filter(x => x.part == this);
    }

    public disconnectAllInternalConnections() {
         for (let c of this.internalPins) {
           for (let cc  of this.internalPins) {
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

    public loadMovablesFormJSON(o :any){
        for (let propName of this.getGetters()){
            // @ts-ignore
            let prop = this[propName];

            if (prop instanceof Movable || prop instanceof ConnectPoint) {
                // @ts-ignore
                this[propName].setFromJSON(o[propName]);
            }
        }
    }

    private getGetters() {
        let list = [];
       let propsD = Object.getOwnPropertyDescriptors(Object.getPrototypeOf(this));
        for (const [key, value] of Object.entries(propsD)) {
           if (value.get != null) {
               list.push(key);
           }

        }
        return list;
    }

}