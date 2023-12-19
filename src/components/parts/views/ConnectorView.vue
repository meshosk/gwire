<script setup lang="ts">
import {onMounted, ref, watch} from "vue";
import {ConnectPoint, DraggableOver, HighlightType} from "@/components/parts/common";

const props = defineProps(['onDragStart','onDraggedOver', "isDraggable", 'connection', 'xShift', 'yShift'])

const connection :ConnectPoint = props?.connection; // wrapped in proxy
if (connection == null) {
  throw new Error("Connection not defined");
}

const cele =  ref<HTMLElement | undefined>(undefined);
const color = ref("gray");

const m = connection.draggable;
m.canStartDrag =  (props.isDraggable != null ? props.isDraggable : true);

if (props.xShift != null) {
  m.xShift.value = props.xShift;
}
if (props.yShift != null) {
  m.yShift.value = props.yShift;
}

m.onDraggedOverAction = props.onDraggedOver;
m.onDraggingStartAction = (item) => {
    if (props.onDragStart != null) {
        props.onDragStart(item);
    }
    color.value = "yellow";
}
let setDefaultState = () => {
    color.value = "gray";
}

m.onDraggingOverEndAction = setDefaultState;
m.onDraggingEndAction = setDefaultState;
m.stopMovingAction = setDefaultState;

m.onDraggingOverAction = (source) => {
    color.value = "green"
}
onMounted(() => {
 m.dropAreaElement = cele;
})

</script>

<template>
  <g :class="HighlightType[m.connectPoint.highlight.value]" >
    <circle :cx="m.xShifted.value" :cy="m.yShifted.value"  ref="cele" r="10" stroke="black" stroke-width="2" :fill="color"
            @mousedown="m.onMouseDown"
    />
  </g>
</template>
<style scoped>

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