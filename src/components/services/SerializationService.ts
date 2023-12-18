import {getCurrentInstance, inject} from 'vue'
import {CircuitPart, ConnectionLockService} from "@/components/parts/common";
export class SerializationService {
    private _app;


    private static _service = null;
    /**
     * Static method for easy injection
     */
    static inject() : SerializationService {
        return this._service;
    }


    constructor() {
        SerializationService._service =  this;
    }

    public saveToFile(parts :CircuitPart[]) {

       let jsonObjets = [];

        for (let part of parts) {
            jsonObjets.push(part.JSONObject);
        }

        let o = {
            app : "Gwire",
            version : 0.1,
            type : "Gwire JSON scheme save",
            name : "My custom scheme",
            parts : jsonObjets,
            locks : ConnectionLockService.inject().JSONObject,
            repo : "https://github.com/meshosk/gwire/",
            www : "not-created",
        };

        const element = document.createElement("a");
        const file = new Blob([JSON.stringify(o)], {type: 'text/JSON'});
        element.href = URL.createObjectURL(file);
        element.download = "scheme.json";
        document.body.appendChild(element); // Required for this to work in FireFox
        element.click();
    }

    public async load() {
        const element = document.createElement("input")
        element.setAttribute("type", "file");

        element.onchange = () => {
            let file = element.files[0];
            let reader = new FileReader();
            reader.onload =  () => {
                // Display the file's contents
                console.log(reader.result);
            };
            reader.readAsText(file);
        }
        element.click();


    }
    public connect(app){
        this._app = app;
    }


}