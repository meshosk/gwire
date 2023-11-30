<script setup lang="ts">
import { ref } from 'vue'

const x = ref(0);
const y = ref(0);

var isDown = false;

function onMouseDown() {
    isDown = true;
}
function onMouseUp() {
   isDown = false;
}

function onMouseMove(e : MouseEvent) {
   if (isDown) {
      x.value += e.movementX;
      y.value += e.movementY;
   }
}

    
</script>

<template>
   <rect :x="x" :y="y" width="300" height="100" style="fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"
      @mousedown="onMouseDown"
      @mouseup="onMouseUp"
      @mousemove="onMouseMove"
      @mouseout="onMouseUp"
   />
</template>