<script setup lang="ts">
import {MouseService} from "@/components/services/MouseServise";
import {EditorService} from "@/components/services/EditorService";
import *  as vueComps from "@/components/parts/views";
import TestCircle from "@/components/parts/views/TestCircle.vue";
import {SerializationService} from "@/components/services/SerializationService";
import {onMounted, ref, toRaw} from "vue";

var mouseService = MouseService.inject();
var editorService = EditorService.inject();
var serializationService = SerializationService.inject();

function add(type :string) {
  editorService.addPart(type);
}

function showRoute(){
  editorService.showRoute();
}


function test() {
  let parts = editorService.parts.value.map(x => toRaw(x));
  serializationService.saveToFile(parts);
}
const getComponent = (s) => {
  let a = vueComps[s];
  return a;
}
</script>

<template>
  <main>
    <div class="menu">
        <button @click='test'>test</button>
        <button @click='showRoute'>Show route</button>
        <button @click='add("CableModel")'>Add cable</button>
        <button @click='add("CircleModel")'>Add circle</button>
        <button @click='add("InputJackModel")'>Add input jack</button>
    </div>
    <svg
        @mousemove="mouseService.onMouseMove" @mousedown="mouseService.onMouseDown"  @mouseup="mouseService.onMouseUp">
        <component v-for="comp in editorService.normalParts.value"  :is='getComponent(comp.vueComponentName)' :model="comp"/>
        <component v-for="(comp,index) in editorService.prioritizedParts.value"  :is='getComponent(comp.vueComponentName)' :model="comp" :key="comp.id"/>
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
