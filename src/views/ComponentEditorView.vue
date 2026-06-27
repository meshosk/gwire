<script setup lang="ts">
import { ref, computed, type Ref } from 'vue';
import { ComponentTemplate, ComponentState, TemplateConnectPoint, DynamicComponentModel } from "@/components/parts/models";
import { EditorService } from "@/components/services/EditorService";
import { SerializationService } from "@/components/services/SerializationService";

// State
const template = ref<ComponentTemplate | null>(null);
const selectedStateId = ref<string>("");
const selectedPinId = ref<string>("");
const isDraggingPin = ref(false);
const dragOffset = ref({ x: 0, y: 0 });

const editorService = EditorService.inject();
const serializationService = SerializationService.inject();

// Initialize with empty template
if (!template.value) {
    template.value = new ComponentTemplate(
        "newComponent",
        "New Component",
        "",
        [],
        [new ComponentState("state1", [])],
        "state1"
    );
    selectedStateId.value = "state1";
}

// Computed
const states = computed(() => template.value?.states || []);
const pins = computed(() => template.value?.pins || []);
const currentState = computed(() => {
    if (!template.value || !selectedStateId.value) return null;
    return template.value.getStateById(selectedStateId.value) || null;
});

const svgWidth = ref(400);
const svgHeight = ref(300);

// Save/Load template file
const saveTemplate = () => {
    if (!template.value) return;
    const json = JSON.stringify(template.value.toJSON(), null, 2);
    const blob = new Blob([json], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${template.value.name}.json`;
    a.click();
    URL.revokeObjectURL(url);
};

const loadTemplate = async () => {
    // Use native file input for loading templates
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';
    input.onchange = async (e) => {
        const file = (e.target as HTMLInputElement).files?.[0];
        if (file) {
            try {
                const jsonStr = await file.text();
                const jsonObj = JSON.parse(jsonStr);
                template.value = ComponentTemplate.fromJSON(jsonObj);
                selectedStateId.value = template.value.defaultState;
                selectedPinId.value = "";
            } catch (err) {
                console.error("Failed to load template:", err);
            }
        }
    };
    input.click();
};

// Add new template
const createNewTemplate = () => {
    template.value = new ComponentTemplate(
        "newComponent_" + Date.now(),
        "New Component",
        "",
        [],
        [new ComponentState("state1", [])],
        "state1"
    );
    selectedStateId.value = "state1";
    selectedPinId.value = "";
};

// State management
const addState = () => {
    if (!template.value) return;
    const stateId = "state" + (template.value.states.length + 1);
    const newState = new ComponentState(stateId, []);
    template.value.states.push(newState);
    selectedStateId.value = stateId;
};

const removeState = (stateId: string) => {
    if (!template.value) return;
    if (template.value.states.length <= 1) {
        alert("Must have at least one state");
        return;
    }
    template.value.states = template.value.states.filter(s => s.id !== stateId);
    if (selectedStateId.value === stateId) {
        selectedStateId.value = template.value.states[0]?.id || "";
    }
};

// Pin management
const addPin = () => {
    if (!template.value) return;
    const pinId = "pin" + (template.value.pins.length + 1);
    const newPin = new TemplateConnectPoint(
        pinId,
        pinId,
        50 + template.value.pins.length * 30,
        50 + template.value.pins.length * 30
    );
    template.value.pins.push(newPin);
    selectedPinId.value = pinId;
};

const removePin = (pinId: string) => {
    if (!template.value) return;
    template.value.pins = template.value.pins.filter(p => p.id !== pinId);
    if (selectedPinId.value === pinId) {
        selectedPinId.value = "";
    }
    // Remove pin from all connections
    for (const state of template.value.states) {
        for (let i = 0; i < state.connections.length; i++) {
            state.connections[i] = state.connections[i].filter(p => p !== pinId);
            if (state.connections[i].length === 0) {
                state.connections.splice(i, 1);
                i--;
            }
        }
    }
};

// Connection management
const addConnectionGroup = () => {
    if (!currentState.value) return;
    currentState.value.connections.push([]);
};

const removeConnectionGroup = (index: number) => {
    if (!currentState.value) return;
    currentState.value.connections.splice(index, 1);
};

const addPinToConnection = (groupIndex: number) => {
    if (!currentState.value) return;
    // Add a placeholder that user can edit
    currentState.value.connections[groupIndex].push("pin" + Date.now());
};

const removePinFromConnection = (groupIndex: number, pinIndex: number) => {
    if (!currentState.value) return;
    currentState.value.connections[groupIndex].splice(pinIndex, 1);
};

// SVG management
const updateSvg = (svg: string) => {
    if (!template.value) return;
    template.value.svg = svg;
};

// Add component to editor
const addToEditor = () => {
    if (!template.value || !template.value.isValid()) {
        alert("Template is not valid. Add SVG and at least one pin.");
        return;
    }
    const dynamicModel = new DynamicComponentModel(template.value);
    dynamicModel.initPosition(200, 200);
    // Add the dynamic model directly to the parts list
    const parts = editorService.parts.value;
    dynamicModel.initPosition(200, 200);
    parts.push(dynamicModel);
};

// Pin dragging on canvas
const onPinMouseDown = (pin: TemplateConnectPoint, e: MouseEvent) => {
    if (!template.value) return;
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
    if (!isDraggingPin.value || !template.value) return;
    const svgEl = document.querySelector('svg.editor-canvas');
    if (svgEl) {
        const rect = svgEl.getBoundingClientRect();
        const x = (e.clientX - rect.left - dragOffset.value.x) * (400 / rect.width);
        const y = (e.clientY - rect.top - dragOffset.value.y) * (300 / rect.height);
        
        const pin = template.value.getPinById(selectedPinId.value);
        if (pin) {
            pin.x = Math.max(0, Math.min(400, x));
            pin.y = Math.max(0, Math.min(300, y));
        }
    }
};

const onCanvasMouseUp = () => {
    isDraggingPin.value = false;
};

// Preview component in editor
const previewModel = ref<DynamicComponentModel | null>(null);

// No preview needed - component is shown in canvas via pins
</script>

<template>
    <div class="component-editor">
        <!-- Left Panel - Templates & States -->
        <div class="panel left-panel">
            <h3>Template</h3>
            <div class="form-group">
                <label>Name:</label>
                <input v-model="template!.name" type="text" />
            </div>
            <div class="form-group">
                <label>Display Name:</label>
                <input v-model="template!.displayName" type="text" />
            </div>
            
            <div class="actions">
                <button @click="createNewTemplate">New</button>
                <button @click="loadTemplate">Load</button>
                <button @click="saveTemplate">Save</button>
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

        <!-- Center Panel - Canvas -->
        <div class="panel center-panel">
            <h3>Component Canvas</h3>
            <svg class="editor-canvas" viewBox="0 0 400 300"
                 @mousemove="onCanvasMouseMove" @mouseup="onCanvasMouseUp">
                <!-- Background -->
                <rect width="400" height="300" fill="#f0f0f0" />
                
                <!-- Template SVG preview -->
                <g v-if="template?.svg" transform="translate(10, 10)">
                    <g v-html="template.svg" />
                </g>
                
                <!-- Empty state placeholder -->
                <text v-else x="200" y="150" text-anchor="middle" fill="#999">
                    Enter SVG graphics
                </text>
                
                <!-- Pins -->
                <g v-for="pin in pins" :key="pin.id">
                    <circle
                        :cx="pin.x / 400 * 400"
                        :cy="pin.y / 300 * 300"
                        r="12"
                        fill="none"
                        stroke="#3e68ff"
                        stroke-width="2"
                        @mousedown="onPinMouseDown(pin, $event)"
                        class="pin-circle"
                    />
                    <text
                        :x="pin.x / 400 * 400"
                        :y="pin.y / 300 * 300 + 4"
                        text-anchor="middle"
                        fill="#333"
                        font-size="10"
                        pointer-events="none"
                    >{{ pin.id }}</text>
                </g>
            </svg>
        </div>

        <!-- Right Panel - Properties -->
        <div class="panel right-panel">
            <h3>SVG Graphics</h3>
            <div class="form-group">
                <label>SVG (inline):</label>
                <textarea v-model="template!.svg" rows="8" placeholder="<svg>...</svg>"></textarea>
            </div>

            <h3>Connections ({{ currentState?.id }})</h3>
            <div v-if="currentState">
                <div v-for="(group, gIndex) in currentState.connections" :key="gIndex" class="connection-group">
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