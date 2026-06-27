import { CircuitPart, ConnectPoint, HighlightType } from "@/components/parts/common";
import { ref, watch, type Ref } from "vue";
import { ComponentTemplate } from "./ComponentTemplate";
import { ComponentState } from "./ComponentState";
import * as Vue from "@vue/reactivity";

/**
 * Runtime model created from a ComponentTemplate.
 * Extends CircuitPart to work with existing editor algorithms.
 * Creates ConnectPoint instances based on template pin definitions.
 * Handles state switching with internal connections.
 */
export class DynamicComponentModel extends CircuitPart {

    readonly circuitPartName: string = "dynamicComponent";

    private readonly _template: ComponentTemplate;
    private readonly _templateName: string;
    
    private _currentState: Ref<string>;
    
    private _svgOverride: Ref<string>;

    constructor(template: ComponentTemplate) {
        // Create pin names from template
        const pinNames = template.pins.map(p => p.id);
        super(pinNames);

        this._template = template;
        this._templateName = template.name;
        this._currentState = ref(template.defaultState);
        this._svgOverride = ref("");

        // Watch for state changes and update connections
        watch(() => this._currentState.value, (stateId) => {
            this.applyState(stateId);
            this.onPartChangeState(this);
        });
    }

    /**
     * Get the template this model was created from
     */
    get template(): ComponentTemplate {
        return this._template;
    }

    /**
     * Get the template name
     */
    get templateName(): string {
        return this._templateName;
    }

    /**
     * Get current state id
     */
    get currentState(): Ref<string> {
        return this._currentState;
    }

    /**
     * Get current state object
     */
    get currentStateObj(): ComponentState | undefined {
        return this._template.getStateById(this._currentState.value);
    }

    /**
     * Get all available states
     */
    get states(): string[] {
        return this._template.states.map(s => s.id);
    }

    /**
     * Get display name of current state
     */
    get currentStateDisplayName(): string {
        const state = this.currentStateObj;
        return state?.id || this._template.defaultState;
    }

    /**
     * Get SVG from template
     */
    get svg(): string {
        return this._template.svg;
    }

    /**
     * Get SVG override (empty string means use template SVG)
     */
    get svgOverride(): Ref<string> {
        return this._svgOverride;
    }

    /**
     * Set the current state by id
     */
    setState(stateId: string): void {
        if (this._template.states.find(s => s.id === stateId)) {
            this._currentState.value = stateId;
        }
    }

    /**
     * Cycle to next state
     */
    nextState(): void {
        const currentIndex = this._template.states.findIndex(s => s.id === this._currentState.value);
        const nextIndex = (currentIndex + 1) % this._template.states.length;
        this.setState(this._template.states[nextIndex].id);
    }

    /**
     * Apply state connections based on connection groups
     * Each group in connections array contains pin ids that are connected together
     */
    private applyState(stateId: string): void {
        const state = this._template.getStateById(stateId);
        if (!state) return;

        // Disconnect all internal connections first
        this.disconnectAllInternalConnections();

        // Apply connections from each group
        for (const connectionGroup of state.connections) {
            if (connectionGroup.length < 2) continue;

            // Get ConnectPoint instances for each pin in the group
            const connectPoints: ConnectPoint[] = [];
            for (const pinId of connectionGroup) {
                const pin = this.pinByName(pinId);
                if (pin) {
                    connectPoints.push(pin);
                }
            }

            // Connect all pins in the group to each other
            for (let i = 0; i < connectPoints.length; i++) {
                for (let j = i + 1; j < connectPoints.length; j++) {
                    connectPoints[i].connect(connectPoints[j]);
                }
            }
        }
    }

    /**
     * Override initPosition to position all pins relative to main movable
     */
    override initPosition(x: number, y: number): void {
        this.m.x.value = x;
        this.m.y.value = y;
        
        // Position pins relative to the component origin based on template
        for (const pinDef of this._template.pins) {
            const pin = this.pinByName(pinDef.id);
            if (pin) {
                pin.draggable.x.value = x + pinDef.x;
                pin.draggable.y.value = y + pinDef.y;
            }
        }
    }

    /**
     * Get JSON object for serialization
     */
    public get JSONObject(): object {
        return {
            id: this.id,
            type: this.nonMinifiedClassName,
            templateName: this._templateName,
            m: this.m.JSONObject,
            currentState: this._currentState.value,
            svgOverride: this._svgOverride.value,
            pins: this.internalPins.map(pin => pin.JSONObject)
        };
    }

    /**
     * Load model from JSON object
     */
    public setFromJSON(o: any): void {
        if (this.nonMinifiedClassName !== o.type) {
            throw new Error("Wrong JSON object type");
        }

        // Load main position
        this.m.setFromJSON(o.m);

        // Load state
        if (o.currentState) {
            this._currentState.value = o.currentState;
        }

        // Load SVG override
        if (o.svgOverride) {
            this._svgOverride.value = o.svgOverride;
        }

        // Load pin positions and restore connections
        this.loadMovablesFormJSON(o);

        // Re-apply connections based on current state
        this.applyState(this._currentState.value);
    }

    get vueComponentName(): string {
        return "DynamicComponentView";
    }

    get nonMinifiedClassName(): string {
        return "DynamicComponentModel";
    }

    /**
     * Get pin positions relative to component origin (for editor)
     */
    getPinRelativePositions(): { id: string; x: number; y: number }[] {
        return this.internalPins.map(pin => {
            const pinDef = this._template.getPinById(pin.name);
            return {
                id: pin.name,
                x: pin.draggable.x.value - this.m.x.value,
                y: pin.draggable.y.value - this.m.y.value
            };
        });
    }

    /**
     * Update pin position relative to component origin
     */
    setPinRelativePosition(pinId: string, x: number, y: number): void {
        const pin = this.pinByName(pinId);
        if (pin) {
            pin.draggable.x.value = this.m.x.value + x;
            pin.draggable.y.value = this.m.y.value + y;
        }
    }
}