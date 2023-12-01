<script setup lang="ts">
import {ref, watch} from 'vue'
import {Movable} from "@/components/basic/Movable";
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
</script>

<template>
  <g  :transform="[  'translate('+m.x.value+', '+m.y.value+')']">
    <circle r="40" stroke="black" stroke-width="3" :fill="color"
          @mousedown="m.onMouseDown"
    />
    <Connector :cx="-10" :cy="-20" />
    <Connector :cx="20" :cy="30" />
  </g>
</template>