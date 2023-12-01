<script setup lang="ts">
import {isRef, ref, Ref, UnwrapRef, watch} from 'vue'
import {Movable} from "@/components/basic/Movable";
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

</script>

<template>
  <g  :transform="[  'translate('+m.x.value+', '+m.y.value+')']">
     <rect width="300" height="100" :style="{fill: color}" style="stroke-width:3;stroke:rgb(0,0,0)"
        @mousedown="m.onMouseDown"
     />
    <Connector :cx="50" :cy="50" />
    <Connector :cx="200" :cy="50" />
  </g>
</template>