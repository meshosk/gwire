import {CircuitPart, Cable} from "@/components/parts";
import * as pparts from "@/components/parts";
import { ref, inject} from "vue";

export class EditorService {

    private _parts  ;



    constructor(private context: Object) {
        this._parts = ref([]);
    }
    static inject()  {
        return inject("EditorService");
    }
    public addPart(partName :string){

        let instance = new pparts[partName]();
        this._parts.value.push(instance);
        return instance;
    }

    public get parts() {
        return this._parts;
    }

}