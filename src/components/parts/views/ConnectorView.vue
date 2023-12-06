<script setup lang="ts">
import {ref, watch} from "vue";
import {DraggableOver} from "@/components/parts/common";
import {CableModel} from "@/components/parts/models";

const props = defineProps(['onDragAction','x', 'y','xShift', 'yShift', "isDraggable", 'model', 'draggableId'])
const model :CableModel = props.model; // wrapped in proxy
if (props.model == null) {
  throw new Error("Model not defined");
}

const emit = defineEmits(["update:x", "update:y"])

const cele =  ref<HTMLElement | undefined>(undefined);
const color = ref("gray");

const m = new DraggableOver(model, props.draggableId, cele);
m.xShift.value = props.xShift;
m.yShift.value = props.yShift;
m.x.value = props.x;
m.y.value = props.y;
m.canStartDrag =  props.isDraggable != null ? props.isDraggable : true;

watch(() => props.x, (n) => {
  m.x.value = props.x;
})
watch(() => props.y,(n) => {
  m.y.value = props.y;
})

watch(() => m.x.value, (n) => {
  emit("update:x", n);
})
watch(() => m.y.value,(n) => {
    emit("update:y", n);
})


m.onDraggedOverAction = props.onDragAction;
let setDefaultState = () => {
    color.value = "gray";
}

m.onDraggingOverEndAction = setDefaultState;
m.onDraggingEndAction = setDefaultState;
m.stopMovingAction = setDefaultState;

m.onDraggingStartAction = () => {
    color.value = "yellow"
}
m.onDraggingOverAction = (source) => {
    color.value = "green"
}

</script>

<template>
  <circle :cx="m.xShifted.value" :cy="m.yShifted.value"  ref="cele" r="10" stroke="black" stroke-width="2" :fill="color"
          @mousedown="m.setAsSource()"
  />
</template>