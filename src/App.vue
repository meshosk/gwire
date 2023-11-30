<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import basic from './components/basic/basic.vue'
</script>

<template>
  <main>
    <div class="menu">
      asdasd TESTs
    </div>
    <svg>
      <basic />
    </svg>
  </main>
</template>

<style scoped lang="scss">

main {
  width: 100vw;
  height: 100vh;
  display: flex;
  align-items: stretch;

  & > .menu {
    width: 280px;
  }

  svg {
    width: 100%;
    height: 100%;
  }

}




</style>
