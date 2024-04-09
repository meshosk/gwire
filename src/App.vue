<script setup lang="ts">
import {MouseService} from "@/components/services/MouseServise";
import {EditorService} from "@/components/services/EditorService";
import *  as vueComps from "@/components/parts/views";
import {SerializationService} from "@/components/services/SerializationService";
import {onMounted, ref, toRaw} from "vue";
import ContextMenuView from "@/components/parts/views/ContextMenuView.vue";
import {ContextMenuService} from "@/components/services/ContextMenuService";
import RouterLinkButton from "@/components/common/RouterLinkButton.vue";
import MenuSpacer from "@/components/common/MenuSpacer.vue";


var editorService = EditorService.inject();
var serializationService = SerializationService.inject();


function save() {
  let parts = editorService.parts.value.map(x  => toRaw(x));
  // @ts-ignore
  serializationService.saveToFile(parts);
}

function load() {
  serializationService.load();
}

</script>

<template>
  <main>
    <nav id="main-menu">
      <div>
        GWire
      </div>
      <button @click='save'>Save</button>
      <button @click='load'>Load</button>
      <MenuSpacer/>
      <RouterLinkButton to="/">
        Editor
      </RouterLinkButton>
      <RouterLinkButton to="/about">
        About
      </RouterLinkButton>
    </nav>
    <RouterView />
  </main>
</template>
<style scoped lang="scss">
main {
  width: 100vw;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: start;
  flex-direction: column;
}
#main-menu {
  width: 100%;
  display: flex;
  align-items: start;
  flex-direction: row;
  background-color: darkgrey;
}

</style>
