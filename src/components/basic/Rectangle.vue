<script setup lang="ts">
import {isRef, ref, Ref, UnwrapRef} from 'vue'
import {Movable} from "@/components/basic/Movable";

const m = new Movable();

</script>

<template>
   <rect :x="m.x.value" :y="m.y.value" width="300" height="100" style="fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"
      @mousedown="m.onMouseDown"
      @mouseup="m.onMouseUp"
      @mousemove="m.onMouseMove"
      @mouseout="m.onMouseUp"
   />
</template>