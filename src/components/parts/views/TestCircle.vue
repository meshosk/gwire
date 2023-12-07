<script setup lang="ts">
import {ConnectPoint, Movable} from "@/components/parts/common";
import Connector from "@/components/parts/views/ConnectorView.vue";

const m = new Movable();
const p = new ConnectPoint(null,"uho");
</script>

<template>
    <circle :cx="m.x.value" :cy="m.y.value" r="40" stroke="black" stroke-width="3" fill="red"
            @mousedown="m.onMouseDown"
    />
    <Connector v-model:x="m.x.value" :x-shift="0" v-model:y="m.y.value" :y-shift="0"
               :connection="p"
    />
</template>

<style scoped>

</style>