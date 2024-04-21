<script setup lang="ts">
import {MouseService} from "@/components/services/MouseServise";
import {EditorService} from "@/components/services/EditorService";
import *  as vueComps from "@/components/parts/views";
import ContextMenuView from "@/components/parts/views/ContextMenuView.vue";
import {ContextMenuItem} from "@/components/parts/common/ContextMenuItem";
import {ContextMenuService} from "@/components/services/ContextMenuService";

const cms =  (ContextMenuService.inject() as ContextMenuService);

var mouseService = (MouseService.inject() as MouseService);
var editorService = EditorService.inject();
function add(type :string) {
  let part = editorService.addPart(type);
  part.initPosition(cms.x.value, cms.y.value);
}

function showRoute(){
  editorService.showRoute();
}

function getComponent (comp: any) {
  // @ts-ignore
  return  vueComps[comp.vueComponentName];
}


const contextMenu = [
  new ContextMenuItem("Add part","", [
    new ContextMenuItem("Circle part","", () => add("CircleModel")),
    new ContextMenuItem("Input jack","", () => add("InputJackModel")),
  ]),
  new ContextMenuItem("Add cable","", () => add("CableModel")),
  new ContextMenuItem("Show route","", () => showRoute()),
];
</script>

<template>
  <context-menu-view  />
  <div class="editor">
    <svg
        @contextmenu.prevent="e => cms.openMenu(e.clientX, e.clientY, contextMenu)"
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
