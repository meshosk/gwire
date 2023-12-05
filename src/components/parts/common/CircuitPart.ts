import {ConnectPoint} from "@/components/parts/common";

export abstract class CircuitPart {

    private readonly _pins :ConnectPoint[] = [];

    public onPartChangeState :(circlePart: CircuitPart) => void = (circlePart) =>{};

    abstract readonly circuitPartName :string;

    public get vueComponentName(): string {
        return this.constructor.name.replace("Model", "View");
    }

    protected constructor(totalPinCount = 1) {
        for (let i = 0; i < totalPinCount; i++) {
            this._pins.push(new ConnectPoint(this));
        }
    }

    public get pins(): ConnectPoint[] {
        return this._pins;
    }

    /**
     * Returns other part that is current
     */
    public getConnectedParts(except :CircuitPart[]) :CircuitPart[]
    {
        return this._pins
            .filter(c1 => c1.part != this  && !this._pins.some(c2 => c2.part == c1.part) )
            .map( x => x.part);
    }

    public disconnectAllInternalConnections() {
        for (let c of this._pins) {
            c.disconnectInternalConnectionsOnly();
        }
    }


}