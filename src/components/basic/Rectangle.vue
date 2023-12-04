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
    source.x.value = target.xShifted.value;
    source.y.value = target.yShifted.value;
}

</script>

<template>
     <rect :x="m.x.value" :y="m.y.value" width="300" height="100" :style="{fill: color}" style="stroke-width:3;stroke:rgb(0,0,0)"
        @mousedown="m.onMouseDown"
     />
    <Connector :x="m.x.value" :x-shift="50" :y="m.y.value" :y-shift="50"  :onDragAction="onDragOver"/>
    <Connector :x="m.x.value" :x-shift="200" :y="m.y.value" :y-shift="50"   :onDragAction="onDragOver"/>
</template>