<script setup lang="ts">
import {ref, watch, defineComponent} from 'vue'
import {Movable, DraggableOver} from "@/components/parts/common";
import ConnectorView from "@/components/parts/views/ConnectorView.vue";
import {CircleModel} from "@/components/parts/models";

const m = new Movable();
const color = ref("blue");
const props = defineProps(['model'])

const model :CircleModel = props.model;

model.onPartChangeState = (model) => {
   // model.ge
}

watch(m.mouseIsDown, async() => {
  if (m.mouseIsDown.value === true) {
    color.value = "yellow";
  } else {
    color.value = "blue";
  }
})

function onDragOver(source :DraggableOver, target :DraggableOver) {
  source.x.value = target.xShifted.value;
  source.y.value = target.yShifted.value;
}

</script>
<template>
    <circle :cx="m.x.value" :cy="m.y.value" r="40" stroke="black" stroke-width="3" :fill="color"
          @mousedown="m.onMouseDown"
    />
    <text :x="m.x.value + 10" :y="m.y.value+10" class="small" style="color: white"
          @mousedown="m.onMouseDown"
    >On</text>
    <ConnectorView :x="m.x.value" :x-shift="-10" :y="m.y.value" :y-shift="-20" :onDragAction="onDragOver"/>
    <ConnectorView :x="m.x.value" :x-shift="20" :y="m.y.value" :y-shift="30" :onDragAction="onDragOver"/>
</template>
<style scoped>
text {
    -webkit-user-select: none; /* Safari */
    -ms-user-select: none; /* IE 10 and IE 11 */
    user-select: none; /* Standard syntax */
}
</style>