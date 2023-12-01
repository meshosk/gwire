<script setup lang="ts">
import {isRef, ref, Ref, UnwrapRef, shallowRef, watch} from 'vue'
import {Movable} from "@/components/basic/Movable";

const m = new Movable();
const color = ref("blue");

watch(m.isDragged, async() => {
  if (m.isDragged.value === true) {
    color.value = "yellow";
  } else {
    color.value = "blue";
  }
})
</script>

<template>
    <circle :cx="m.x.value" :cy="m.y.value" r="40" stroke="black" stroke-width="3" :fill="color"
          @mousedown="m.onMouseDown"
    />
</template>