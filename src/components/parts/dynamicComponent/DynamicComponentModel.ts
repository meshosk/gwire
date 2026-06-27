import { CircuitPart, ConnectPoint, HighlightType } from "@/components/parts/base";
import { ref, watch, type Ref } from "vue";
import { DynamicComponentState } from "@/components/parts/dynamicComponent/DynamicComponentState";

export class DynamicComponentModel extends CircuitPart {

    readonly circuitPartName: string = "dynamicComponent";

    private readonly _name: string;
    private readonly _creatorName: string;
    private readonly _createdAt: Date;
    private readonly _states: DynamicComponentState[];
    private readonly _pinPositions: { id: string; x: number; y: number }[];
    
    private _currentState: Ref<string>;
    private _currentStateSVG: Ref<string>;

    constructor(name: string, states: DynamicComponentState[], pinNames: string[], pinPositions: { id: string; x: number; y: number }[], creatorName: string = "", createdAt?: Date) {
        super(pinNames);

        this._name = name;
        this._creatorName = creatorName;
        this._createdAt = createdAt || new Date();
        this._states = states;
        this._pinPositions = pinPositions;
        this._currentState = ref(states[0]?.id || "");
        this._currentStateSVG = ref(states[0]?.svg || "");

        watch(() => this._currentState.value, (stateId) => {
            this.applyState(stateId);
            this.onPartChangeState(this);
            const state = this._states.find(s => s.id === stateId);
            this._currentStateSVG.value = state?.svg || this._states[0]?.svg || "";
        });
    }

    get name(): string {
        return this._name;
    }

    get creatorName(): string {
        return this._creatorName;
    }

    get createdAt(): Date {
        return this._createdAt;
    }

    get states(): DynamicComponentState[] {
        return this._states;
    }

    get stateIds(): string[] {
        return this._states.map(s => s.id);
    }

    get currentState(): Ref<string> {
        return this._currentState;
    }

    get currentStateObj(): DynamicComponentState | undefined {
        return this._states.find(s => s.id === this._currentState.value);
    }

    get currentStateSVG(): Ref<string> {
        return this._currentStateSVG;
    }

    setState(stateId: string): void {
        if (this._states.find(s => s.id === stateId)) {
            this._currentState.value = stateId;
        }
    }

    nextState(): void {
        const currentIndex = this._states.findIndex(s => s.id === this._currentState.value);
        const nextIndex = (currentIndex + 1) % this._states.length;
        this.setState(this._states[nextIndex].id);
    }

    private applyState(stateId: string): void {
        const state = this._states.find(s => s.id === stateId);
        if (!state) return;

        this.disconnectAllInternalConnections();

        for (const connectionGroup of state.connectionGroups) {
            if (connectionGroup.length < 2) continue;

            const connectPoints: ConnectPoint[] = [];
            for (const pinId of connectionGroup) {
                const pin = this.pinByName(pinId);
                if (pin) {
                    connectPoints.push(pin);
                }
            }

            for (let i = 0; i < connectPoints.length; i++) {
                for (let j = i + 1; j < connectPoints.length; j++) {
                    connectPoints[i].connect(connectPoints[j]);
                }
            }
        }
    }

    override initPosition(x: number, y: number): void {
        this.m.x.value = x;
        this.m.y.value = y;

        for (const pinDef of this._pinPositions) {
            const pin = this.pinByName(pinDef.id);
            if (pin) {
                pin.draggable.x.value = x + pinDef.x;
                pin.draggable.y.value = y + pinDef.y;
            }
        }
    }

    public get JSONObject(): object {
        return {
            id: this.id,
            type: this.nonMinifiedClassName,
            name: this._name,
            creatorName: this._creatorName,
            createdAt: this._createdAt.toISOString(),
            m: this.m.JSONObject,
            currentState: this._currentState.value,
            pins: this.internalPins.map(pin => pin.JSONObject)
        };
    }

    public setFromJSON(o: any): void {
        if (this.nonMinifiedClassName !== o.type) {
            throw new Error("Wrong JSON object type");
        }

        this.m.setFromJSON(o.m);

        if (o.currentState && this._states.find(s => s.id === o.currentState)) {
            this._currentState.value = o.currentState;
        }

        this.loadMovablesFormJSON(o);

        this.applyState(this._currentState.value);
    }

    get vueComponentName(): string {
        return "DynamicComponentView";
    }

    get nonMinifiedClassName(): string {
        return "DynamicComponentModel";
    }

    getPinRelativePositions(): { id: string; x: number; y: number }[] {
        return this.internalPins.map(pin => {
            return {
                id: pin.name,
                x: pin.draggable.x.value - this.m.x.value,
                y: pin.draggable.y.value - this.m.y.value
            };
        });
    }

    setPinRelativePosition(pinId: string, x: number, y: number): void {
        const pin = this.pinByName(pinId);
        if (pin) {
            pin.draggable.x.value = this.m.x.value + x;
            pin.draggable.y.value = this.m.y.value + y;
        }
    }
}
