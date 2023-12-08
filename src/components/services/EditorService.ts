import * as modelRef from "@/components/parts/models";
import {ref, inject} from "vue";
import  * as Vue from "@vue/reactivity"
import {CircuitPart} from "@/components/parts/common";

export class EditorService {

    private readonly _parts  :Vue.Ref<Vue.UnwrapRef<CircuitPart[]>>;

    constructor() {
        this._parts = ref([]);
    }
    static inject() :EditorService  {
        return <EditorService>inject("EditorService");
    }
    public addPart(partName :string){
        let instance = new modelRef[partName]();
         if (instance == null) {
             throw new Error(`Model for '${partName}' was not found`);
         }
        this._parts.value.push(instance);
        return instance;
    }

    public get parts() :Vue.Ref<Vue.UnwrapRef<CircuitPart[]>> {
        return this._parts;
    }

    public get normalParts() :Vue.Ref<Vue.UnwrapRef<CircuitPart[]>> {
        return this._parts.value.filter(x => !x.drawPriority)
    }

    public get prioritizedParts() {
        return this._parts.value.filter(x => x.drawPriority)
    }

}