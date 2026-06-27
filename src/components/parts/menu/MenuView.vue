<script setup lang="ts">
import {toRaw} from "vue";
import {ContextMenuService} from "@/components/services/ContextMenuService";

const cms = ContextMenuService.inject();
const props = defineProps(['items'])

</script>

<template>
  <ul class="menu">
    <li  class="menu-item" :class="(item.subMenu && Array.isArray(item.subMenu) ? 'has-submenu' : '')" v-for="item in props.items">
      <div
           @mouseenter="() => { item.subMenuIsOpen.value = true }"
           @mouseleave="item.subMenuIsOpen.value = false"
        @click="(() => { if(item.subMenu && typeof item.subMenu === 'function') {item.subMenu()}; cms.closeMenu(); })"
      >
        <div v-if="item.iconView" class="menu-item-component">
          <component :is="item.iconView"/>
        </div>
        <div  class="menu-item-label">
          {{ item.text }}
        </div>
        <menu-view class="sub-menu"  v-if="item.subMenuIsOpen.value && item.subMenu && Array.isArray(item.subMenu) == true"
                    :items="item.subMenu"  :key="item.id" />
<!--        <menu-view class="sub-menu"  v-if="item.subMenu && Array.isArray(item.subMenu) == true"-->
<!--                   :items="item.subMenu"  :key="item.id" />-->
      </div>
    </li>
  </ul>
</template>

<style lang="scss">
  .menu-item {
      display: block;
      position: relative;
      border: 1px solid black;
      background-color: darkgray;
      min-width: 80px;
  }
  .menu .menu.sub-menu {
    position: absolute;
    top: 0;
    left: 100%;
  }

  .menu ul {
    display: flex;
    flex-direction: column;
    padding: 0;
    margin: 0;
    & li {
      display: block;
      padding: 2px 4px;
    }
  }

  .has-submenu > div > .menu-item-label {
    &::after {
      content: ">";
      position: absolute;
      right: 0;
      padding-right: 5px;
      padding-left: 5px;
    }
  }

</style>