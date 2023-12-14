import {getCurrentInstance, inject} from 'vue'
import {CircuitPart} from "@/components/parts/common";
export class SerializationService {
    private _app;


    static inject() : SerializationService {
        return (inject("SerializationService") as SerializationService);
    }


    constructor() {

    }

    public saveToFile(parts :CircuitPart[]) {

       let jsonString = "";

        for (let part of parts) {

        }


    }

    public load(json :string) {

    }
    public connect(app){
        this._app = app;
    }


}