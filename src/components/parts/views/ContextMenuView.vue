<script setup lang="ts">

import {ContextMenuService} from "@/components/services/ContextMenuService";
import MenuView from "@/components/parts/views/MenuView.vue";
import {toRaw} from "vue";

const cms = ContextMenuService.inject();

</script>

<template>
<div v-if="cms.menuIsOpen.value" id="context-menu"
     :style="`left: ${cms.x.value}px; top: ${cms.y.value}px`"
     @onblur="cms.closeMenu">
     <menu-view :items="toRaw(cms.menuItems.value)" />
</div>

</template>

<style scoped lang="scss">
#context-menu {
  position: absolute;
  z-index: 9999;
}
</style>