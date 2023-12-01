<script setup lang="ts">
import {isRef, ref, Ref, UnwrapRef, shallowRef, watch} from 'vue'
import {Movable} from "@/components/basic/Movable";

//const m = new Movable();
const color = ref("gray");

</script>

<template>
  <circle r="10" stroke="black" stroke-width="2" :fill="color" />
</template>