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
  let b = vueComps['Cable'];
  let a = markRaw(Cable);
}

const getComponent = (s) => {
  let a = this[s];
  return a;
}

</script>

<template>
  <main>
    <div class="menu">
      asdasd TESTs xxxx
        <button @click='add("Cable")'>Add cable</button>
<!--        <button @click='add("Circle")'>Add circle</button>-->
      <ul>
        <li v-for="part in editorService.parts.value">
          {{ part.vueComponentName }}
        </li>
      </ul>
    </div>
    <svg @mousemove="mouseService.onMouseMove" @mousedown="mouseService.onMouseDown"  @mouseup="mouseService.onMouseUp">
        <component v-for="t in editorService.parts.value"  :is='getComponent("Cable")'/>

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
