<script setup lang="ts">

import {ConnectionLockService, DraggableOver, HighlightType, Movable} from "@/components/parts/common";
import Connector from "@/components/parts/views/ConnectorView.vue";
import {CableModel} from "@/components/parts/models";
import {EditorService} from "@/components/services/EditorService";
import {toRaw} from "vue";

// model prop contains background ref to Circuit part, vue comp is used as view only
// for this reason it only react on model change
const props = defineProps(['model'])
const model :CableModel = toRaw(props.model); // wrapped in proxy
if (props.model == null) {
    throw new Error("Model not defined");
}

const connectionLock =  ConnectionLockService.inject();
const editorService = EditorService.inject();


model.c1.x.value = 100;
model.c1.y.value = 100;

model.c2.x.value = 300;
model.c2.y.value = 100;

const onDragStart = (item) => {
    connectionLock.releaseAllLockFor(item);
}

function onDragOver(source :DraggableOver, target :DraggableOver) {
  source.x.value = target.xShifted.value;
  source.y.value = target.yShifted.value;

    // make model connection
    source.connectPoint.connect(target.connectPoint);
    // make connection
    connectionLock.lock(
        source, target,
        () => [target.xShifted.value, target.yShifted.value],
        () => {
            source.x.value = target.xShifted.value;
            source.y.value = target.yShifted.value;
        },
        () => {
            source.connectPoint.disconnect(target.connectPoint)
        }
    )
}

const makeOnTop = (make : boolean) => {
  model.highlight.value = make ? HighlightType.SELECTED : HighlightType.NONE;
  editorService.makeOnTop(model);
}


</script>

<template>
    <g
        @mousedown="makeOnTop(true)"
        @blur ="makeOnTop(false)"
        :class="HighlightType[model.highlight.value]">

    <line :x1="model.c1.x.value" :y1="model.c1.y.value" :x2="model.c2.x.value" :y2="model.c2.y.value" stroke="black" stroke-width="4" />
    <Connector v-model:x="model.c1.x.value" :x-shift="0" v-model:y="model.c1.y.value" :y-shift="0"
               :onDraggedOver="onDragOver"
               :on-drag-start="onDragStart"
               :connection="model.pins[0]"
    />
    <Connector v-model:x="model.c2.x.value" :x-shift="0" v-model:y="model.c2.y.value" :y-shift="0"
               :onDraggedOver="onDragOver"
               :on-drag-start="onDragStart"
               :connection="model.pins[1]"
    />
    </g>
</template>

<style scoped>
  line {
    z-index: 1;
  }

  g:focus {
     outline:  none;
  }

  g.SELECTED > * {
      filter:
              drop-shadow(-1px -1px 0px #3e68ff)
              drop-shadow(2px -1px 0px #3e68ff)
              drop-shadow(2px 2px 0px #3e68ff)
              drop-shadow(-1px 2px 0px #3e68ff)

  }
  g.ROUTE > * {
      filter:
              drop-shadow(-1px -1px 0px red)
              drop-shadow(2px -1px 0px red)
              drop-shadow(2px 2px 0px red)
              drop-shadow(-1px 2px 0px red)

  }
</style>