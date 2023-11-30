<script setup lang="ts">
import { ref } from 'vue'

var x = ref(0);
var y = ref(0);

function onMousedown(e) {
    x = e.offseX;
    y = e.offseY;
}                

    
</script>

<template>
   <rect x="{{ x }}" y="{{ y }}" width="300" height="100" style="fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)" @mousedown="onMouseDown" />
</template>