
import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import {MouseService} from "@/components/services/MouseServise";
import {EditorService} from "@/components/services/EditorService";
import {ConnectionLockService} from "@/components/services/ConnectionLockService";
import {SerializationService} from "@/components/services/SerializationService";

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.provide("MouseService", new MouseService());
app.provide("EditorService", new EditorService());
app.provide("ConnectionLockService", new ConnectionLockService());
app.provide("SerializationService", new SerializationService());

app.config.errorHandler = (err, instance, info) => {
    console.log(err);
}


app.mount('#app')
