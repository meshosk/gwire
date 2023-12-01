<script setup lang="ts">
import {ref} from "vue";
import {DraggableOver} from "@/components/basic/DraggableOver";


const m = new DraggableOver();
const color = ref("gray");

</script>

<template>
  <circle r="10" stroke="black" stroke-width="2" :fill="color"
          @mousedown="m.setAsSource()"
          @mouseenter="m.setAsTarget()"
          @mouseleave="m.clearTarget()"
  />
</template>