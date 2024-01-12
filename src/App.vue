<script setup lang="ts">
import {MouseService} from "@/components/services/MouseServise";
import {EditorService} from "@/components/services/EditorService";
import *  as vueComps from "@/components/parts/views";
import {SerializationService} from "@/components/services/SerializationService";
import {onMounted, ref, toRaw} from "vue";
import ContextMenuView from "@/components/parts/views/ContextMenuView.vue";
import {ContextMenuService} from "@/components/services/ContextMenuService";



var mouseService = MouseService.inject();
var editorService = EditorService.inject();
var serializationService = SerializationService.inject();
var cms = ContextMenuService.inject();

function add(type :string) {
  editorService.addPart(type);
}

function showRoute(){
  editorService.showRoute();
}

function save() {
  let parts = editorService.parts.value.map(x  => toRaw(x));
  // @ts-ignore
  serializationService.saveToFile(parts);
}

function load() {
  serializationService.load();
}
function getComponent (comp: any) {
  // @ts-ignore
  return  vueComps[comp.vueComponentName];
}
</script>

<template>

  <main>
    <context-menu-view  />
    <div class="menu">
        <button @click='save'>Save</button>
        <button @click='load'>Load</button>
        <button @click='showRoute'>Show route</button>
        <button @click='add("CableModel")'>Add cable</button>
        <button @click='add("CircleModel")'>Add circle</button>
        <button @click='add("InputJackModel")'>Add input jack</button>
    </div>
    <svg
        @mousemove="mouseService.onMouseMove" @mousedown="mouseService.onMouseDown"  @mouseup="mouseService.onMouseUp">
        <component v-for="comp in editorService.normalParts.value"  :is="getComponent(comp)" :model="comp" :key="comp.id"/>
        <component v-for="comp in editorService.prioritizedParts.value"  :is="getComponent(comp)" :model="comp" :key="comp.id"/>
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
