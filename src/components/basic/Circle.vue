<script setup lang="ts">
import {ref, watch} from 'vue'
import {Movable, DraggableOver} from "@/components/basic/index";
import Connector from "@/components/basic/Connector.vue";

const m = new Movable();
const color = ref("blue");

watch(m.mouseIsDown, async() => {
  if (m.mouseIsDown.value === true) {
    color.value = "yellow";
  } else {
    color.value = "blue";
  }
})

function onDragOver(source :DraggableOver, target :DraggableOver) {
  source.x.value = target.x.value;
  source.y.value = target.y.value;
}

</script>

<template>

    <circle :cx="m.x.value" :cy="m.y.value" r="40" stroke="black" stroke-width="3" :fill="color"
          @mousedown="m.onMouseDown"
    />
    <Connector :x="m.x.value" :x-shift="-10" :y="m.y.value" :y-shift="-20" :onDragAction="onDragOver"/>
    <Connector :x="m.x.value" :x-shift="20" :y="m.y.value" :y-shift="30" :onDragAction="onDragOver"/>

</template>