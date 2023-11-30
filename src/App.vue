<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import Rectangle from './components/basic/Rectangle.vue'
import {ref} from "vue"
import Circle from "@/components/basic/Circle.vue";

const comps = ref([]);
var ids = 1;
var r = false;
function add() {

    if (r) {
        comps.value.push(Rectangle)
    } else {
        comps.value.push(Circle)
    }
    r = !r;
}

</script>

<template>
  <main>
    <div class="menu">
      asdasd TESTs
        <button @click="add">Add</button>
    </div>
    <svg>
        <component v-for="comp in comps" :is="comp"/>
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
