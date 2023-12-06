<script setup lang="ts">

import {DraggableOver} from "@/components/parts/common";
import {ref } from "vue";
import Connector from "@/components/parts/views/ConnectorView.vue";
import {CableModel} from "@/components/parts/models";

// model prop contains background ref to Circuit part, vue comp is used as view only
// for this reason it only react on model change
const props = defineProps(['model'])
const model :CableModel = props.model; // wrapped in proxy
if (props.model == null) {
    throw new Error("Model not defined");
}

const c1ele = ref<HTMLElement | undefined>(undefined);
const c1 = new DraggableOver(model, "c1", c1ele);

const c2ele = ref<HTMLElement | undefined>(undefined);
const c2 = new DraggableOver(model, "c2", c2ele);

c1.x.value = 100;
c1.y.value = 100;

c2.x.value = 300;
c2.y.value = 100;

function onDragOver(source :DraggableOver, target :DraggableOver) {
  source.x.value = target.xShifted.value;
  source.y.value = target.yShifted.value;
}

</script>

<template>
    <line :x1="c1.x.value" :y1="c1.y.value" :x2="c2.x.value" :y2="c2.y.value" stroke="black" stroke-width="2" />

    <Connector v-model:x="c1.x.value" :x-shift="0" v-model:y="c1.y.value" :y-shift="0"
               @mousedown="(e) => {c1.onMouseDown(e); c1.setAsSource()}"
               :onDragAction="onDragOver"
               :model="model"
               :draggable-id="'c1'"
    />

    <Connector v-model:x="c2.x.value" :x-shift="0" v-model:y="c2.y.value" :y-shift="0"
               @mousedown="(e) => {c2.onMouseDown(e); c2.setAsSource()}"
               :onDragAction="onDragOver"
               :model="model"
               :draggable-id="'c1'"
    />
</template>

<style scoped>

</style>