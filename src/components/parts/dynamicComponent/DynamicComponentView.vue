<script setup lang="ts">
import { ref, toRaw, watch } from 'vue';
import { HighlightType } from "@/components/parts/base";
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

function onDragOver(source: any, target: any) {
    source.x.value = target.xShifted.value;
    source.y.value = target.yShifted.value;
}

const getStateContextMenu = (): ContextMenuItem[] => {
    const items: ContextMenuItem[] = [];
    for (const stateId of model.stateIds) {
        items.push(new ContextMenuItem(stateId, "", () => {
            model.setState(stateId);
        }));
    }
    return items;
};
</script>

<template>
    <g
        @mousedown="makeOnTop(true)"
        @blur="makeOnTop(false)"
        :class="HighlightType[model.highlight.value]">
        
        <g :transform="`translate(${model.m.x.value} ${model.m.y.value})`">
            <g v-if="model.currentStateSVG.value" v-html="model.currentStateSVG.value" />
        </g>
        
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
