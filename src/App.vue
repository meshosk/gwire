<script setup lang="ts">
import {markRaw, ref, shallowRef} from "vue"
import {MouseService} from "@/components/MouseServise";
import {EditorService} from "@/components/EditorService";
import *  as vueComps from "@/components/basic";
import Cable from "@/components/basic/Cable.vue";
import {CircuitPart} from "@/components/parts";


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
        <button @click='add("Cable")'>Add cable</button>
    </div>
    <svg @mousemove="mouseService.onMouseMove" @mousedown="mouseService.onMouseDown"  @mouseup="mouseService.onMouseUp">
        <component v-for="comp in editorService.parts.value"  :is='getComponent(comp.vueComponentName)'/>
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
