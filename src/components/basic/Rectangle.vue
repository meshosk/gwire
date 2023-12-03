<script setup lang="ts">
import {isRef, ref, Ref, UnwrapRef, watch} from 'vue'
import {DraggableOver, Movable} from "@/components/basic/index";
import Connector from "@/components/basic/Connector.vue";

const m = new Movable();
const color = ref("red");
watch(m.mouseIsDown, async() => {
  if (m.mouseIsDown.value === true) {
    color.value = "yellow";
  } else {
    color.value = "red";
  }
})
function onDragOver(source :DraggableOver, target :DraggableOver) {
    source.x.value = m.x.value + target.x.value;
    source.y.value = m.y.value + target.y.value;
}

</script>

<template>
  <g  :transform="[  'translate('+m.x.value+', '+m.y.value+')']">
     <rect width="300" height="100" :style="{fill: color}" style="stroke-width:3;stroke:rgb(0,0,0)"
        @mousedown="m.onMouseDown"
     />
    <Connector :x="50" :y="50"  :onDragAction="onDragOver"/>
    <Connector :x="200" :y="50"  :onDragAction="onDragOver"/>
  </g>
</template>