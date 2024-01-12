<script setup lang="ts">

import {ContextMenuService} from "@/components/services/ContextMenuService";
import MenuView from "@/components/parts/views/MenuView.vue";

const cms = ContextMenuService.inject();

</script>

<template>
<div v-if="cms.menuIsOpen.value" id="context-menu"
     :style="`left: ${cms.x.value}px; top: ${cms.y.value}px`"
     @onblur="cms.closeMenu">
     <menu-view :items="cms.menuItems" />
</div>

</template>

<style scoped lang="scss">
#context-menu {
  position: absolute;
  z-index: 9999;
  border: 1px solid black;
  background-color: blue;
  width: 300px;
  height: 100px;
}
</style>