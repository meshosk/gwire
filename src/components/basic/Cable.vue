<script setup lang="ts">

import {DraggableOver} from "@/components/basic/index";
import {ref} from "vue";
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

    <Connector :x="c1.x.value" :x-shift="0" :y="c1.y.value" :y-shift="0"
               @mousedown="(e) => {c1.onMouseDown(e); c1.setAsSource()}"
    />

    <Connector :x="c2.x.value" :x-shift="0" :y="c2.y.value" :y-shift="0"
               @mousedown="(e) => {c2.onMouseDown(e); c2.setAsSource()}"
    />


<!--    <circle ref="c1ele" r="10" stroke="black" stroke-width="2" :fill="color"-->
<!--            :cx="c1.x.value"-->
<!--            :cy="c1.y.value"-->
<!--            @mousedown="(e) => {c1.onMouseDown(e); c1.setAsSource()}"-->
<!--    />-->
<!--    <circle ref="c2ele" r="10" stroke="black" stroke-width="2" :fill="color"-->
<!--            :cx="c2.x.value"-->
<!--            :cy="c2.y.value"-->
<!--            @mousedown="(e) => {c2.onMouseDown(e); c2.setAsSource()}"-->

<!--    />-->
</template>

<style scoped>

</style>