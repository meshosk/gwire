import {ConnectPoint, CircuitPart} from "@/components/parts/common";
import {ref, watch} from "vue";

export enum Switch3WayState {
    Position1,
    Position2,
    Position3,
}
export class Switch3WayModel extends CircuitPart {

    readonly circuitPartName: "3 way switch";

    private _position  = ref(Switch3WayState.Position1);

    constructor() {
    super([1,2,3,4,5,6]);
        watch( () => this._position, (value) => {
            switch (value) {
                case Switch3WayState.Position1:
                    this.setPointsToPosition1();
                    return;
                case Switch3WayState.Position2:
                    this.setPointsToPosition2();
                    return;
                default:
                case Switch3WayState.Position3:
                    this.setPointsToPosition3();
            }
        })
    }

    get position() {
        return this._position;
    }

    private setPointsToPosition1() {
        this.disconnectAllInternalConnections();
        this.pins[0].connect(this.pins[2]);
    }
    private setPointsToPosition2() {
        this.disconnectAllInternalConnections();
        this.pins[0].connect(this.pins[2]);
        this.pins[1].connect(this.pins[3]);
    }
    private setPointsToPosition3() {
        this.disconnectAllInternalConnections();
        this.pins[1].connect(this.pins[3]);
    }



}