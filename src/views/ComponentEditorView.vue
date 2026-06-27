<script setup lang="ts">
import { ref, computed, type Ref } from 'vue';
import { DynamicComponentState } from "@/components/parts/dynamicComponent/DynamicComponentState";
import { DynamicComponentModel } from "@/components/parts/dynamicComponent/DynamicComponentModel";
import { EditorService } from "@/components/services/EditorService";
import { SerializationService } from "@/components/services/SerializationService";

const componentName = ref("newComponent");
const displayName = ref("New Component");
const states = ref<DynamicComponentState[]>([]);
const pins = ref<{ id: string; label: string; x: number; y: number }[]>([]);
const selectedStateId = ref<string>("");
const selectedPinId = ref<string>("");
const isDraggingPin = ref(false);
const dragOffset = ref({ x: 0, y: 0 });

const editorService = EditorService.inject();
const serializationService = SerializationService.inject();

const newStateId = () => "state" + (states.value.length + 1);

if (states.value.length === 0) {
    states.value.push(new DynamicComponentState("state1", "", []));
    selectedStateId.value = "state1";
}

const currentState = computed(() => {
    if (!selectedStateId.value) return null;
    return states.value.find(s => s.id === selectedStateId.value) || null;
});

const svgWidth = ref(400);
const svgHeight = ref(300);

const saveComponent = () => {
    const json = JSON.stringify({
        name: componentName.value,
        displayName: displayName.value,
        states: states.value.map(s => s.toJSON()),
        pinNames: pins.value.map(p => p.id),
        pinPositions: pins.value.map(p => ({ id: p.id, x: p.x, y: p.y }))
    }, null, 2);
    const blob = new Blob([json], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${componentName.value}.json`;
    a.click();
    URL.revokeObjectURL(url);
};

const loadComponent = async () => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';
    input.onchange = async (e) => {
        const file = (e.target as HTMLInputElement).files?.[0];
        if (file) {
            try {
                const jsonStr = await file.text();
                const jsonObj = JSON.parse(jsonStr);
                componentName.value = jsonObj.name || "loadedComponent";
                displayName.value = jsonObj.displayName || jsonObj.name || "Loaded Component";
                states.value = (jsonObj.states || []).map((s: any) => DynamicComponentState.fromJSON(s));
                pins.value = (jsonObj.pinPositions || []).map((p: any) => ({
                    id: p.id,
                    label: p.id,
                    x: p.x,
                    y: p.y
                }));
                selectedStateId.value = states.value[0]?.id || "";
                selectedPinId.value = "";
            } catch (err) {
                console.error("Failed to load component:", err);
            }
        }
    };
    input.click();
};

const createNewTemplate = () => {
    componentName.value = "newComponent_" + Date.now();
    displayName.value = "New Component";
    states.value = [new DynamicComponentState("state1", "", [])];
    pins.value = [];
    selectedStateId.value = "state1";
    selectedPinId.value = "";
};

const addState = () => {
    const stateId = newStateId();
    states.value.push(new DynamicComponentState(stateId, "", []));
    selectedStateId.value = stateId;
};

const removeState = (stateId: string) => {
    if (states.value.length <= 1) {
        alert("Must have at least one state");
        return;
    }
    states.value = states.value.filter(s => s.id !== stateId);
    if (selectedStateId.value === stateId) {
        selectedStateId.value = states.value[0]?.id || "";
    }
};

const addPin = () => {
    const pinId = "pin" + (pins.value.length + 1);
    pins.value.push({
        id: pinId,
        label: pinId,
        x: 50 + pins.value.length * 30,
        y: 50 + pins.value.length * 30
    });
    selectedPinId.value = pinId;
};

const removePin = (pinId: string) => {
    pins.value = pins.value.filter(p => p.id !== pinId);
    if (selectedPinId.value === pinId) {
        selectedPinId.value = "";
    }
    for (const state of states.value) {
        for (let i = 0; i < state.connectionGroups.length; i++) {
            state.connectionGroups[i] = state.connectionGroups[i].filter(p => p !== pinId);
            if (state.connectionGroups[i].length === 0) {
                state.connectionGroups.splice(i, 1);
                i--;
            }
        }
    }
};

const addConnectionGroup = () => {
    if (!currentState.value) return;
    currentState.value.connectionGroups.push([]);
};

const removeConnectionGroup = (index: number) => {
    if (!currentState.value) return;
    currentState.value.connectionGroups.splice(index, 1);
};

const addPinToConnection = (groupIndex: number) => {
    if (!currentState.value) return;
    currentState.value.connectionGroups[groupIndex].push("pin" + Date.now());
};

const removePinFromConnection = (groupIndex: number, pinIndex: number) => {
    if (!currentState.value) return;
    currentState.value.connectionGroups[groupIndex].splice(pinIndex, 1);
};

const updateStateSvg = (stateId: string, svg: string) => {
    const state = states.value.find(s => s.id === stateId);
    if (state) {
        state.svg = svg;
    }
};

const updateStateId = (stateId: string, newId: string) => {
    const state = states.value.find(s => s.id === stateId);
    if (state) {
        const oldId = state.id;
        state.id = newId;
        if (selectedStateId.value === oldId) {
            selectedStateId.value = newId;
        }
        for (const group of state.connectionGroups) {
            for (let i = 0; i < group.length; i++) {
                if (group[i] === oldId) {
                    group[i] = newId;
                }
            }
        }
    }
};

const addToEditor = () => {
    if (pins.value.length === 0) {
        alert("Component must have at least one pin.");
        return;
    }
    const pinNames = pins.value.map(p => p.id);
    const pinPositions = pins.value.map(p => ({ id: p.id, x: p.x, y: p.y }));
    const dynamicModel = new DynamicComponentModel(
        componentName.value,
        states.value,
        pinNames,
        pinPositions,
        "",
        new Date()
    );
    dynamicModel.initPosition(200, 200);
    const parts = editorService.parts.value;
    parts.push(dynamicModel);
};

const onPinMouseDown = (pin: { id: string; x: number; y: number }, e: MouseEvent) => {
    isDraggingPin.value = true;
    selectedPinId.value = pin.id;
    const svgEl = document.querySelector('svg.editor-canvas');
    if (svgEl) {
        const rect = svgEl.getBoundingClientRect();
        dragOffset.value = {
            x: e.clientX - rect.left - (pin.x * (svgWidth.value / 400)),
            y: e.clientY - rect.top - (pin.y * (svgHeight.value / 300))
        };
    }
};

const onCanvasMouseMove = (e: MouseEvent) => {
    if (!isDraggingPin.value) return;
    const svgEl = document.querySelector('svg.editor-canvas');
    if (svgEl) {
        const rect = svgEl.getBoundingClientRect();
        const x = (e.clientX - rect.left - dragOffset.value.x) * (400 / rect.width);
        const y = (e.clientY - rect.top - dragOffset.value.y) * (300 / rect.height);
        
        const pin = pins.value.find(p => p.id === selectedPinId.value);
        if (pin) {
            pin.x = Math.max(0, Math.min(400, x));
            pin.y = Math.max(0, Math.min(300, y));
        }
    }
};

const onCanvasMouseUp = () => {
    isDraggingPin.value = false;
};

const updatePinPosition = (pinId: string, x: number, y: number) => {
    const pin = pins.value.find(p => p.id === pinId);
    if (pin) {
        pin.x = x;
        pin.y = y;
    }
};
</script>

<template>
    <div class="component-editor">
        <div class="panel left-panel">
            <h3>Component</h3>
            <div class="form-group">
                <label>Name:</label>
                <input v-model="componentName" type="text" />
            </div>
            <div class="form-group">
                <label>Display Name:</label>
                <input v-model="displayName" type="text" />
            </div>
            
            <div class="actions">
                <button @click="createNewTemplate">New</button>
                <button @click="loadComponent">Load</button>
                <button @click="saveComponent">Save</button>
                <button @click="addToEditor" class="primary">Add to Editor</button>
            </div>

            <h3>Pins</h3>
            <button @click="addPin">Add Pin</button>
            <div v-for="pin in pins" :key="pin.id" class="pin-item"
                 :class="{ selected: selectedPinId === pin.id }"
                 @click="selectedPinId = pin.id">
                <span>{{ pin.id }}</span>
                <span>x: {{ Math.round(pin.x) }}, y: {{ Math.round(pin.y) }}</span>
                <button @click.stop="removePin(pin.id)">×</button>
            </div>

            <h3>States</h3>
            <button @click="addState">Add State</button>
            <div v-for="state in states" :key="state.id" class="state-item"
                 :class="{ selected: selectedStateId === state.id }"
                 @click="selectedStateId = state.id">
                <span>{{ state.id }}</span>
                <button @click.stop="removeState(state.id)" v-if="states.length > 1">×</button>
            </div>
        </div>

        <div class="panel center-panel">
            <h3>Component Canvas</h3>
            <svg class="editor-canvas" viewBox="0 0 400 300"
                 @mousemove="onCanvasMouseMove" @mouseup="onCanvasMouseUp">
                <rect width="400" height="300" fill="#f0f0f0" />
                
                <g v-if="currentState?.svg" transform="translate(10, 10)">
                    <g v-html="currentState.svg" />
                </g>
                
                <text v-else x="200" y="150" text-anchor="middle" fill="#999">
                    Enter SVG graphics in state properties
                </text>
                
                <g v-for="pin in pins" :key="pin.id">
                    <circle
                        :cx="pin.x"
                        :cy="pin.y"
                        r="12"
                        fill="none"
                        stroke="#3e68ff"
                        stroke-width="2"
                        @mousedown="onPinMouseDown(pin, $event)"
                        class="pin-circle"
                    />
                    <text
                        :x="pin.x"
                        :y="pin.y + 4"
                        text-anchor="middle"
                        fill="#333"
                        font-size="10"
                        pointer-events="none"
                    >{{ pin.id }}</text>
                </g>
            </svg>
        </div>

        <div class="panel right-panel">
            <h3>State Properties</h3>
            <div v-if="currentState">
                <div class="form-group">
                    <label>State ID:</label>
                    <input :value="currentState.id" @input="updateStateId(currentState.id, ($event.target as HTMLInputElement).value)" type="text" />
                </div>
                
                <h4>SVG Graphics</h4>
                <div class="form-group">
                    <textarea v-model="currentState.svg" rows="6" placeholder="<svg>...</svg>"></textarea>
                </div>

                <h4>Connections</h4>
                <div v-for="(group, gIndex) in currentState.connectionGroups" :key="gIndex" class="connection-group">
                    <div class="group-header">
                        <span>Group {{ gIndex + 1 }}</span>
                        <button @click="removeConnectionGroup(gIndex)">×</button>
                    </div>
                    <div v-for="(pinId, pIndex) in group" :key="pIndex" class="connection-item">
                        <span>{{ pinId }}</span>
                        <button @click="removePinFromConnection(gIndex, pIndex)">×</button>
                    </div>
                    <button @click="addPinToConnection(gIndex)">+ Add Pin</button>
                </div>
                <button @click="addConnectionGroup">+ Add Group</button>
            </div>
            <div v-else>
                <p>Select a state to edit properties.</p>
            </div>
        </div>
    </div>
</template>

<style scoped lang="scss">
.component-editor {
    display: flex;
    height: 100%;
    width: 100%;

    .panel {
        padding: 10px;
        overflow-y: auto;

        h3 {
            margin: 0 0 10px 0;
            font-size: 14px;
            color: #3e68ff;
        }
        
        h4 {
            margin: 10px 0 5px 0;
            font-size: 12px;
            color: #666;
        }
    }

    .left-panel {
        width: 250px;
        background: #f5f5f5;
        border-right: 1px solid #ddd;

        .form-group {
            margin-bottom: 8px;
            
            label {
                display: block;
                font-size: 12px;
                margin-bottom: 2px;
            }
            
            input {
                width: 100%;
                padding: 4px;
                border: 1px solid #ccc;
                border-radius: 3px;
            }
        }

        .actions {
            display: flex;
            flex-wrap: wrap;
            gap: 4px;
            margin: 10px 0;

            button {
                padding: 5px 8px;
                border: 1px solid #ccc;
                border-radius: 3px;
                background: white;
                cursor: pointer;
                font-size: 12px;

                &.primary {
                    background: #3e68ff;
                    color: white;
                    border-color: #3e68ff;
                }

                &:hover {
                    opacity: 0.8;
                }
            }
        }

        .pin-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 5px 8px;
            margin: 2px 0;
            background: white;
            border: 1px solid #ddd;
            border-radius: 3px;
            cursor: pointer;
            font-size: 12px;

            &.selected {
                border-color: #3e68ff;
                background: #e8eeff;
            }

            button {
                padding: 2px 6px;
                border: none;
                background: none;
                cursor: pointer;
                color: red;
                font-weight: bold;
            }
        }

        .state-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 5px 8px;
            margin: 2px 0;
            background: white;
            border: 1px solid #ddd;
            border-radius: 3px;
            cursor: pointer;
            font-size: 12px;

            &.selected {
                border-color: #3e68ff;
                background: #e8eeff;
            }

            button {
                padding: 2px 6px;
                border: none;
                background: none;
                cursor: pointer;
                color: red;
                font-weight: bold;
            }
        }
    }

    .center-panel {
        flex: 1;
        display: flex;
        flex-direction: column;

        svg.editor-canvas {
            flex: 1;
            border: 1px solid #ccc;
            background: white;
            cursor: crosshair;

            .pin-circle {
                cursor: move;

                &:hover {
                    stroke: #ff6b3e;
                    stroke-width: 3;
                }
            }
        }
    }

    .right-panel {
        width: 300px;
        background: #f5f5f5;
        border-left: 1px solid #ddd;

        .form-group {
            margin-bottom: 10px;
            
            label {
                display: block;
                font-size: 12px;
                margin-bottom: 2px;
            }
            
            textarea {
                width: 100%;
                padding: 4px;
                border: 1px solid #ccc;
                border-radius: 3px;
                font-family: monospace;
                font-size: 11px;
                resize: vertical;
            }
        }

        .connection-group {
            margin-bottom: 8px;
            padding: 8px;
            background: white;
            border: 1px solid #ddd;
            border-radius: 3px;

            .group-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                margin-bottom: 5px;
                font-size: 12px;
                font-weight: bold;

                button {
                    padding: 2px 6px;
                    border: none;
                    background: none;
                    cursor: pointer;
                    color: red;
                    font-weight: bold;
                }
            }

            .connection-item {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 3px 5px;
                margin: 2px 0;
                background: #f9f9f9;
                border-radius: 2px;
                font-size: 11px;

                button {
                    padding: 1px 5px;
                    border: none;
                    background: none;
                    cursor: pointer;
                    color: red;
                }
            }

            button {
                margin-top: 5px;
                padding: 3px 8px;
                border: 1px solid #ccc;
                border-radius: 3px;
                background: white;
                cursor: pointer;
                font-size: 11px;

                &:hover {
                    background: #f0f0f0;
                }
            }
        }
    }
}
</style>
