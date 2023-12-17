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

       let jsonObjets = [];

        for (let part of parts) {
            jsonObjets.push(part.JSONObject);
        }

        const element = document.createElement("a");
        const file = new Blob([JSON.stringify(jsonObjets)], {type: 'text/JSON'});
        element.href = URL.createObjectURL(file);
        element.download = "scheme.json";
        document.body.appendChild(element); // Required for this to work in FireFox
        element.click();
    }

    public async load() {
        const element = document.createElement("input")
        element.setAttribute("type", "file");

        element.onchange = () => {
            let file = element.files[0]
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