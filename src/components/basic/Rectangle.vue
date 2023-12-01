<script setup lang="ts">
import {isRef, ref, Ref, UnwrapRef, watch} from 'vue'
import {Movable} from "@/components/basic/Movable";

const m = new Movable();
const color = ref("red");
watch(m.isDragged, async() => {
  if (m.isDragged.value === true) {
    color.value = "yellow";
  } else {
    color.value = "red";
  }
})

</script>

<template>
   <rect :x="m.x.value" :y="m.y.value" width="300" height="100" :style="{fill: color}" style="stroke-width:3;stroke:rgb(0,0,0)"
      @mousedown="m.onMouseDown"
   />
</template>