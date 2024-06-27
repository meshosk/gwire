<script setup lang="ts">

import {ConnectionLockService, Draggable, HighlightType, Movable} from "@/components/parts/common";
import Connector from "@/components/parts/views/ConnectorView.vue";
import {CableModel} from "@/components/parts/models";
import {EditorService} from "@/components/services/EditorService";
import {ref, toRaw, watch, isProxy} from "vue";
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
const color = ref(model.color);

const onDragStart = (item :Draggable) => {
    connectionLock.releaseAllLockFor(item);
}

function onDragOver(source :Draggable, target :Draggable) {
  connectionLock.lock(source, target);

  // position correction, when cable is connected on cable
  source.x.value = target.xShifted.value;
  source.y.value = target.yShifted.value;
}

const makeOnTop = (make : boolean) => {
  model.highlight.value = make ? HighlightType.SELECTED : HighlightType.NONE;
  editorService.makeOnTop(model);
}

watch(color, () => {
  model.color = color.value;
})

const callOnProxyonMouseDown = ( mmm : Movable) => {
   isProxy(mmm) ? toRaw(mmm).onMouseDown(null) : mmm.onMouseDown(null);
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
  new ContextMenuItem("split","", () => model.addSplit(cms.x.value, cms.y.value))
];

</script>

<template>
    <g
        @mousedown="makeOnTop(true)"
        @blur ="makeOnTop(false)"
        :class="HighlightType[model.highlight.value]">

<!--      <template v-for="n in model.splits.value.length" v-if="model.splits.value.length > 0">-->
<!--        <line-->
<!--            v-if="n == 1"-->
<!--            @contextmenu.prevent="e => cms.openMenu(e.clientX, e.clientY, contextMenu)"-->
<!--            :x1="model.c1.draggable.x.value" :y1="model.c1.draggable.y.value"-->
<!--            :x2="model.splits.value[0].x" :y2="model.splits.value[0].y"-->
<!--            :stroke="color" stroke-width="4"-->
<!--        />-->
<!--        <line-->
<!--            v-if="n == model.splits.value.length"-->
<!--            @contextmenu.prevent="e => cms.openMenu(e.clientX, e.clientY, contextMenu)"-->
<!--            :x2="model.c2.draggable.x.value" :y2="model.c2.draggable.y.value"-->
<!--            :x1="model.splits.value[model.splits.value.length-1].x" :y1="model.splits.value[model.splits.value.length-1].y"-->
<!--            :stroke="color" stroke-width="4"-->
<!--        />-->
<!--        <line-->
<!--            v-if="n != 1 && n != model.splits.value.length"-->
<!--            @contextmenu.prevent="e => cms.openMenu(e.clientX, e.clientY, contextMenu)"-->
<!--            :x1="model.splits.value[n-1].x" :y1="model.splits.value[n-1].y"-->
<!--            :x2="model.splits.value[n].x" :y2="model.splits.value[n].y"-->
<!--            :stroke="color" stroke-width="4"-->
<!--        />-->
<!--      </template>-->
      <line
          v-if="model.splits.value.length == 0"
          @contextmenu.prevent="e => cms.openMenu(e.clientX, e.clientY, contextMenu)"
          :x1="model.c1.draggable.x.value" :y1="model.c1.draggable.y.value"
          :x2="model.c2.draggable.x.value" :y2="model.c2.draggable.y.value"
          :stroke="color" stroke-width="4"
      />

      <polyline
          @contextmenu.prevent="e => cms.openMenu(e.clientX, e.clientY, contextMenu)"
          :points="model.polyLinePointsString"
          :stroke="color" stroke-width="4"
          fill="none"
      />

      <circle
          v-for="split in toRaw(model.splits.value)"
          :cx="split.x.value" :cy="split.y.value" r="10" stroke="black" stroke-width="3" :fill="color"
          @mousedown="() => callOnProxyonMouseDown(split)"
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
              drop-shadow(1px -1px 0px #3e68ff)
              drop-shadow(1px 1px 0px #3e68ff)
              drop-shadow(-1px 1px 0px #3e68ff)

  }
  g.ROUTE > * {
      filter:
              drop-shadow(-1px -1px 0px red)
              drop-shadow(1px -1px 0px red)
              drop-shadow(1px 1px 0px red)
              drop-shadow(-1px 1px 0px red)

  }
</style>