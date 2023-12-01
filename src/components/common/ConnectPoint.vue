<script setup lang="ts">
import {ref} from "vue/dist/vue";

const x = ref(0);
const y = ref(0);

</script>

<template>
    <circle cx="{{x}}" cy="{{y}}" r="40" stroke="black" stroke-width="3" fill="red" />
</template>

<style scoped>

</style>