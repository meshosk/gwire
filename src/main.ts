import { createApp } from 'vue'
import { createPinia } from 'pinia'

// Import all of Bootstrap's J & icons
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap/dist/js/bootstrap.js' // onlu one import, otherwise dropdown do not work
import "bootstrap-icons/font/bootstrap-icons.css"

// apps
import App from './App.vue'
import router from './router'
import {MouseService} from "@/components/services/MouseServise";
import {EditorService} from "@/components/services/EditorService";
import {ConnectionLockService} from "@/components/services/ConnectionLockService";
import {SerializationService} from "@/components/services/SerializationService";
import {ContextMenuService} from "@/components/services/ContextMenuService";


const app = createApp(App)

app.use(createPinia())
app.use(router)

app.provide("MouseService", new MouseService());
app.provide("EditorService", new EditorService());
app.provide("ConnectionLockService", new ConnectionLockService());
app.provide("SerializationService", new SerializationService());
app.provide("ContextMenuService", new ContextMenuService());

app.config.errorHandler = (err, instance, info) => {
    console.log(err);
}


app.mount('#app')
