//import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import {MouseService} from "@/components/services/MouseServise";
import {EditorService} from "@/components/services/EditorService";

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.provide("MouseService", new MouseService());
app.provide("EditorService", new EditorService());


app.mount('#app')
