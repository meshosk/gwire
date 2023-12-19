import {getCurrentInstance, inject} from 'vue'
import {CircuitPart, ConnectionLockService, ConnectPoint} from "@/components/parts/common";
import {EditorService} from "@/components/services/EditorService";
import {CableModel} from "@/components/parts/models";
export class SerializationService {


    private static _service :SerializationService
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
            repo : "https://github.com/meshosk/gwire/",
            www : "https://meshosk.github.io/gwire/",
            parts : jsonObjets
        };

        const element = document.createElement("a");
        const file = new Blob([JSON.stringify(o)], {type: 'text/JSON'});
        element.href = URL.createObjectURL(file);
        element.download = "scheme.json";
        document.body.appendChild(element); // Required for this to work in FireFox
        element.click();
    }

    public async load() {

        let JSONString = await this.loadFile();
        let JSONObject = JSON.parse(JSONString);

        let editorService = EditorService.inject();
        let connectionLockService = ConnectionLockService.inject();
        let addedParts = new Map<string, CircuitPart>();
        //TODO check version

        // release all locks
        connectionLockService.releaseAllLock();
        //clean existing parts
        editorService.clearParts();

        // add parts
        for (let obj of JSONObject.parts) {
           let created = editorService.addPart(obj.type);
           created.setFromJSON(obj);
           addedParts.set(obj.id, created);
        }

        // create connections
        for (let obj of JSONObject.parts) {
            let part = addedParts.get(obj.id);

            if (part) {
                for (let internalPin of part.internalPins) {
                    let pinConnections = obj[internalPin.name];
                    if (pinConnections != null) {
                        for (let connectedToPin of pinConnections.connectedTo) {
                            let toConnectPart = addedParts.get(connectedToPin.part);
                            let toConnectedPin = toConnectPart?.pinByName(connectedToPin.name);
                            if (toConnectedPin) {
                                internalPin.connect(toConnectedPin);

                                if (internalPin.part instanceof CableModel) {
                                    connectionLockService.lock(internalPin.draggable, toConnectedPin.draggable);
                                } else {

                                    connectionLockService.lock(toConnectedPin.draggable, internalPin.draggable);
                                }
                            }
                        }
                    }
                }
            }
        }


    }

    private loadFile() :Promise<string> {
        return new Promise( (resolve, reject) => {
            const element = document.createElement("input")
            element.setAttribute("type", "file");
            element.onchange = () => {
                let file = element["files"] ? element["files"][0] : null;
                if (file) {
                    let reader = new FileReader();
                    reader.onload =  () => {
                        // Display the file's contents
                        resolve(typeof reader.result == "string" ? <string>reader.result : "");
                    };
                    reader.readAsText(file);
                }
            }
            element.click();
        })
    }


}