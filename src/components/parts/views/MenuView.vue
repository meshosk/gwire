<script setup lang="ts">
import {toRaw} from "vue";
import {ContextMenuService} from "@/components/services/ContextMenuService";

const cms = ContextMenuService.inject();
const props = defineProps(['items'])
const i = toRaw(props.items);

</script>

<template>
  <ul>
    <li v-for="item in props.items">
      <div class="menu-item"
           @mouseenter="() => { item.subMenuIsOpen.value = true }"
           @mouseleave="item.subMenuIsOpen.value = false"
        @click="(() => { if(item.subMenu && typeof item.subMenu === 'function') {item.subMenu()}; cms.closeMenu(); })"
      >
<!--        <div v-if="item.iconView">-->
<!--          <component :is="item.iconView"/>-->
<!--        </div>-->
        <div>
          {{ item.text }}
        </div>
        <menu-view  v-if="item.subMenuIsOpen.value && item.subMenu && Array.isArray(item.subMenu) == true"
                    :items="item.subMenu"  :key="item.id" />
      </div>
    </li>
  </ul>
</template>

<style scoped>

</style>