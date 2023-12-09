<script setup lang="ts">
import {MouseService} from "@/components/services/MouseServise";
import {EditorService} from "@/components/services/EditorService";
import *  as vueComps from "@/components/parts/views";
import TestCircle from "@/components/parts/views/TestCircle.vue";


var mouseService = MouseService.inject();
var editorService = EditorService.inject();

function add(type :string) {
  editorService.addPart(type);
}

const getComponent = (s) => {
  let a = vueComps[s];
  return a;
}


</script>

<template>
  <main>
    <div class="menu">
        <button @click='add("CableModel")'>Add cable</button>
        <button @click='add("CircleModel")'>Add circle</button>
        <button @click='add("InputJackModel")'>Add input jack</button>
    </div>
    <svg @mousemove="mouseService.onMouseMove" @mousedown="mouseService.onMouseDown"  @mouseup="mouseService.onMouseUp">
        <test-circle />
        <component v-for="comp in editorService.normalParts"  :is='getComponent(comp.vueComponentName)' :model="comp"/>
        <component v-for="comp in editorService.prioritizedParts"  :is='getComponent(comp.vueComponentName)' :model="comp"/>
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
