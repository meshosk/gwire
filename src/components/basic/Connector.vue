<script setup lang="ts">
import {ref, watch} from "vue";
import {DraggableOver} from "@/components/basic/DraggableOver";

const props = defineProps(['onDragAction','x', 'y','xShift', 'yShift'])

const cele =  ref<HTMLElement | undefined>(undefined);
const color = ref("gray");
const m = new DraggableOver(cele);
m.x.value = props.x + props.xShift;
m.y.value = props.y + props.yShift;

watch(() => props.x, (n) => {
  m.x.value = props.x + props.xShift;
})
watch(() => props.y,(n) => {
  m.y.value = props.y + props.yShift;
})

m.onDraggedOverAction = props.onDragAction;
let setDefaultState = () => {
    color.value = "gray";
}

m.onDraggingOverEndAction = setDefaultState;
m.onDraggingEndtAction = setDefaultState;
m.stopMovingAction = setDefaultState;

m.onDraggingStartAction = () => {
    color.value = "yellow"
}
m.onDraggingOverAction = (source) => {
    color.value = "green"

}

</script>

<template>
  <circle :cy="m.y.value" :cx="m.x.value" ref="cele" r="10" stroke="black" stroke-width="2" :fill="color"
          @mousedown="m.setAsSource()"
  />
</template>