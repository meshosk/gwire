<script setup lang="ts">

import {ConnectionLockService, DraggableOver, HighlightType, Movable} from "@/components/parts/common";
import Connector from "@/components/parts/views/ConnectorView.vue";
import {CableModel} from "@/components/parts/models";
import {EditorService} from "@/components/services/EditorService";
import {ref, toRaw} from "vue";
import {ContextMenuService} from "@/components/services/ContextMenuService";
import {ContextMenuItem} from "@/components/parts/common/ContextMenuItem";

// model prop contains background ref to Circuit part, vue comp is used as view only
// for this reason it only react on model change
const props = defineProps(['model'])
const cms =  ContextMenuService.inject();

const model :CableModel = toRaw(props.model); // wrapped in proxy
if (props.model == null) {
    throw new Error("Model not defined");
}

const connectionLock =  ConnectionLockService.inject();
const editorService = EditorService.inject();
const color = ref("black");

const onDragStart = (item :DraggableOver) => {
    connectionLock.releaseAllLockFor(item);
}

function onDragOver(source :DraggableOver, target :DraggableOver) {
  connectionLock.lock(source, target);

  // position correction, when cable is connected on cable
  source.x.value = target.xShifted.value;
  source.y.value = target.yShifted.value;
}

const makeOnTop = (make : boolean) => {
  model.highlight.value = make ? HighlightType.SELECTED : HighlightType.NONE;
  editorService.makeOnTop(model);
}

const contextMenu = [
  new ContextMenuItem("Color", "", [
    new ContextMenuItem("red", "", () => color.value = "red"),
    new ContextMenuItem("blue", "", () => color.value = "blue"),
    new ContextMenuItem("green", "", () => color.value = "green"),
      new ContextMenuItem("more colors", "", [
        new ContextMenuItem("orange", "", () => color.value = "orange"),
        new ContextMenuItem("yellow", "", () => color.value = "yellow"),
      ])
  ]),
  new ContextMenuItem("test","", null)
];

</script>

<template>
    <g
        @contextmenu.prevent="e => cms.openMenu(e.clientX, e.clientY, contextMenu)"
        @mousedown="makeOnTop(true)"
        @blur ="makeOnTop(false)"
        :class="HighlightType[model.highlight.value]">
      <line
          :x1="model.c1.draggable.x.value" :y1="model.c1.draggable.y.value"
          :x2="model.c2.draggable.x.value" :y2="model.c2.draggable.y.value"
          :stroke="color" stroke-width="4"
      />
      <Connector
         :onDraggedOver="onDragOver"
         :on-drag-start="onDragStart"
         :connection="model.c1"
      />
      <Connector
         :onDraggedOver="onDragOver"
         :on-drag-start="onDragStart"
         :connection="model.c2"
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