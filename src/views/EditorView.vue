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

function getComponent (comp: any) {
  // @ts-ignore
  return  vueComps[comp.vueComponentName];
}
</script>

<template>
  <context-menu-view  />
  <div class="editor">
    <div class="editor-menu">
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
  </div>
</template>
<style scoped lang="scss">
.editor {

  display: flex;

  width: 100%;
  height: 100%;

  .editor-menu {
    display: flex;
    flex-direction: column;
    width: 100px;
    background-color: #3e68ff;
  }

  svg {
    width: 100%;
    height: 100%;
  }

}
</style>
