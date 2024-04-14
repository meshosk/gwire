<script setup lang="ts">
import {ref, toRaw, watch} from 'vue'
import {Movable, Draggable,} from "@/components/parts/common";
import ConnectorView from "@/components/parts/views/ConnectorView.vue";
import {CircleModel} from "@/components/parts/models";

const color = ref("blue");
const props = defineProps(['model'])
const model :CircleModel = toRaw(props.model); // wrapped in proxy

watch(model.m.mouseIsDown, async() => {
  if (model.m.mouseIsDown.value === true) {
    color.value = "yellow";
  } else {
    color.value = "blue";
  }
})

function onDragOver(source :Draggable, target :Draggable) {
  source.x.value = target.xShifted.value;
  source.y.value = target.yShifted.value;
}

</script>
<template>
    <circle :cx="model.m.x.value" :cy="model.m.y.value" r="40" stroke="black" stroke-width="3" :fill="color"
          @mousedown="model.m.onMouseDown"
    />
  <g :transform="`translate(${model.m.x.value} ${model.m.y.value - 10})`" @click="() =>  model.isPressed.value =  !model.isPressed.value">
    <rect x="0" y="0" fill="black" width="50" height="20" ></rect>
    <text  x="3" y="17"     >
      {{ model.isPressed.value ? "ON" : "OFF" }}</text>
  </g>
    <ConnectorView
        :XShift="-10" :yShift="-30"
        :isDraggable="false"
        :onDraggedOver="onDragOver"
        :connection="model.s1"
    />
    <ConnectorView
        :xShift="20" :yShift="30"
        :isDraggable="false"
        :onDraggedOver="onDragOver"
        :connection="model.s2"
    />
</template>
<style scoped>
text {
    -webkit-user-select: none; /* Safari */
    -ms-user-select: none; /* IE 10 and IE 11 */
    user-select: none; /* Standard syntax */
  fill: white;
  font-size: 20px;
  background-color: dimgray;
}
</style>