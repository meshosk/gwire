<script setup lang="ts">
import { ref, toRaw, watch } from 'vue';
import { Movable, Draggable, HighlightType } from "@/components/parts/base";
import ConnectorView from "@/components/parts/common/ConnectorView.vue";
import { DynamicComponentModel } from "@/components/parts/dynamicComponent/DynamicComponentModel";
import { EditorService } from "@/components/services/EditorService";
import { ContextMenuService } from "@/components/services/ContextMenuService";
import { ContextMenuItem } from "@/components/parts/common/ContextMenuItem";

const props = defineProps(['model'])
const model: DynamicComponentModel = toRaw(props.model);
const editorService = EditorService.inject();
const cms = ContextMenuService.inject();

const color = ref("blue");
const makeOnTop = (make: boolean) => {
    model.highlight.value = make ? HighlightType.SELECTED : HighlightType.NONE;
    if (make) {
        editorService.makeOnTop(model);
    }
};

const onDragStart = (item: Draggable) => {
    // Could add connection lock logic here if needed
};

function onDragOver(source: Draggable, target: Draggable) {
    source.x.value = target.xShifted.value;
    source.y.value = target.yShifted.value;
}

// State switching context menu
const getStateContextMenu = (): ContextMenuItem[] => {
    const items: ContextMenuItem[] = [];
    for (const stateId of model.states) {
        items.push(new ContextMenuItem(stateId, "", () => {
            model.setState(stateId);
        }));
    }
    return items;
};

const callOnProxyonMouseDown = (mmm: Movable) => {
    const isProxy = typeof mmm === 'object' && mmm !== null && '$' in mmm;
    isProxy ? toRaw(mmm).onMouseDown(null) : mmm.onMouseDown(null);
};
</script>

<template>
    <g
        @mousedown="makeOnTop(true)"
        @blur="makeOnTop(false)"
        :class="HighlightType[model.highlight.value]">
        
        <!-- Main SVG from template -->
        <g :transform="`translate(${model.m.x.value} ${model.m.y.value})`">
            <!-- Render SVG content inline -->
            <g v-html="model.svg" />
        </g>
        
        <!-- Render SVG override if set (replaces template SVG) -->
        <g v-if="model.svgOverride" 
           :transform="`translate(${model.m.x.value} ${model.m.y.value})`">
            <g v-html="model.svgOverride" />
        </g>
        
        <!-- Connect points -->
        <template v-for="pin in model.internalPins" :key="pin.name">
            <ConnectorView
                :xShift="pin.draggable.x.value - model.m.x.value"
                :yShift="pin.draggable.y.value - model.m.y.value"
                :is-draggable="false"
                :onDraggedOver="onDragOver"
                :connection="pin"
            />
        </template>
    </g>
</template>

<style scoped>
g:focus {
    outline: none;
}

g.SELECTED > * {
    filter:
        drop-shadow(-1px -1px 0px #3e68ff)
        drop-shadow(1px -1px 0px #3e68ff)
        drop-shadow(1px 1px 0px #3e68ff)
        drop-shadow(-1px 1px 0px #3e68ff);
}

g.ROUTE > * {
    filter:
        drop-shadow(-1px -1px 0px red)
        drop-shadow(1px -1px 0px red)
        drop-shadow(1px 1px 0px red)
        drop-shadow(-1px 1px 0px red);
}
</style>