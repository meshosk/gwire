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
    <nav class="navbar navbar-expand navbar-dark bg-dark" aria-label="Second navbar example">
      <div class="container-fluid">
        <a class="navbar-brand" href="#">GWire</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarsExample02" aria-controls="navbarsExample02" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="collapse navbar-collapse" id="navbarsExample02">
          <ul class="navbar-nav me-auto">
            <li class="nav-item">
              <router-link to="/" class="nav-link">
                  Editor
              </router-link>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#" @click='save'><i class="bi bi-floppy"></i> Save</a>
            </li>
            <li class="nav-item dropdown">
              <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                Load
              </a>
              <ul class="dropdown-menu">
                <li><a class="dropdown-item" href="#" @click='load'><i class="bi bi-cloud-arrow-up"></i> Load from file</a></li>
                <li><a class="dropdown-item disabled" href="#" disabled>Load from url</a></li>
                <li><hr class="dropdown-divider" ></li>
                <li><a class="dropdown-item disabled" href="#" disabled>Load from library</a></li>
              </ul>
            </li>
            <li class="nav-item">
              <a class="nav-link disabled" href="#" disabled>PDF</a>
            </li>
            <li class="nav-item">
              <router-link to="/about" class="nav-link">
                About
              </router-link>
            </li>
          </ul>
          <div>
            a
          </div>
        </div>
      </div>
    </nav>
    <RouterView />
  </main>
</template>
<style scoped lang="scss">


</style>
