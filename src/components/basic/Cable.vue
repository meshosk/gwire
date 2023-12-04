<script setup lang="ts">

import {DraggableOver} from "@/components/basic/index";
import {ref, onMounted } from "vue";
import Connector from "@/components/basic/Connector.vue";

const c1ele  = ref<HTMLElement | undefined>(undefined);
const c1 = new DraggableOver(c1ele);

const c2ele  = ref<HTMLElement | undefined>(undefined);
const c2 = new DraggableOver(c2ele);

const color = ref("gray");

c1.x.value = 100;
c1.y.value = 100;

c2.x.value = 300;
c2.y.value = 100;

</script>

<template>
    <line :x1="c1.x.value" :y1="c1.y.value" :x2="c2.x.value" :y2="c2.y.value" stroke="black" stroke-width="2" />

    <Connector v-model:x="c1.x.value" :x-shift="0" v-model:y="c1.y.value" :y-shift="0"
               @mousedown="(e) => {c1.onMouseDown(e); c1.setAsSource()}"
    />

    <Connector v-model:x="c2.x.value" :x-shift="0" v-model:y="c2.y.value" :y-shift="0"
               @mousedown="(e) => {c2.onMouseDown(e); c2.setAsSource()}"
    />
</template>

<style scoped>

</style>