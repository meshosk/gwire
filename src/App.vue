<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import Rectangle from './components/basic/Rectangle.vue'
import {markRaw, ref, shallowRef} from "vue"
import Circle from "@/components/basic/Circle.vue";
import {MouseService} from "@/components/MouseServise";
import Cable from "@/components/basic/Cable.vue";

const comps = ref([]);
var ids = 1;
var r = false;
var mouseService = MouseService.inject();

function add() {

    if (r) {
        comps.value.push(markRaw(Rectangle))
    } else {
        comps.value.push(markRaw(Circle))
    }
    r = !r;
}

</script>

<template>
  <main>
    <div class="menu">
      asdasd TESTs xxxx
        <button @click="add">Add</button>
    </div>
    <svg @mousemove="mouseService.onMouseMove" @mousedown="mouseService.onMouseDown"  @mouseup="mouseService.onMouseUp">
        <component v-for="comp in comps" :is="comp"/>
        <cable/>
        <cable/>
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
