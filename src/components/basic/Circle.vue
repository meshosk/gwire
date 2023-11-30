<script setup lang="ts">
import {isRef, ref, Ref, UnwrapRef} from 'vue'
import {Movable} from "@/components/basic/Movable";

const m = new Movable();

</script>

<template>
    <circle :cx="m.x.value" :cy="m.y.value" r="40" stroke="black" stroke-width="3" fill="red"
          @mousedown="m.onMouseDown"
          @mouseup="m.onMouseUp"
          @mousemove="m.onMouseMove"
          @mouseout="m.onMouseUp"
    />
</template>