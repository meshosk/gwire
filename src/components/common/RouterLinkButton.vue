<script setup lang="ts">
const props = defineProps(['to'])
</script>
<template>
  <RouterLink :to="props.to"  v-slot="{ navigate }">
    <button
        @click="navigate"
        role="link"
    >
      <slot></slot>
    </button>
  </RouterLink>
</template>