import * as modelRef from "@/components/parts/models";
import {ref, inject, toRaw} from "vue";
import  * as Vue from "@vue/reactivity"
import {CircuitPart, ConnectPoint, HighlightType} from "@/components/parts/common";
import {InputJackModel} from "@/components/parts/models";

export class EditorService {

    private readonly _parts  :Vue.Ref<Vue.UnwrapRef<CircuitPart[]>>;

    private readonly _partsNormal  :Vue.Ref<Vue.UnwrapRef<CircuitPart[]>> =  ref([])
    private readonly _partsPrioritized  :Vue.Ref<Vue.UnwrapRef<CircuitPart[]>> =  ref([])

    constructor() {
        this._parts = ref([]);
        EditorService._service =  this;
    }

    private static _service :EditorService;
    /**
     * Static method for easy injection
     */
    static inject() : EditorService {
        return this._service;
    }

    public addPart(partName :string){
        // @ts-ignore
        let instance = new modelRef[partName]();
         if (instance == null) {
             throw new Error(`Model for '${partName}' was not found`);
         }
        this._parts.value.push(instance);
        this.reloadTempCollections();
        return instance;
    }

    public clearParts(){
        this._parts.value = [];
        this.reloadTempCollections();
    }

    public get parts()  {
        return this._parts;
    }

    public get normalParts()  {
        return this._partsNormal
    }

    public get prioritizedParts() {
        return this._partsPrioritized
    }

    public makeOnTop(part: any){
        let index = this._parts.value.indexOf(part);
        if (index >-1) {
            this._parts.value.splice(index, 1);
        }
        this._parts.value.push(part);
        this.reloadTempCollections();
    }

    private reloadTempCollections() {
        this._partsNormal.value = this._parts.value.filter(x => x.drawPriority === false);
        this._partsPrioritized.value = this._parts.value.filter(x => x.drawPriority === true)
    }

    public showRoute(){

        for (let part of this._parts.value) {
            part.highlight = HighlightType.NONE;
            for (let pin of part.pins) {
                pin.highlight = HighlightType.NONE;
            }
        }

        // find input jack
        let jacks = this._parts.value.filter( x => x instanceof InputJackModel);
        let pinBag :ConnectPoint[] = [];

        for (let jack of jacks) {

            let startingPin = jack.pins.find(x => x.name == "hot");

            this.recursiveRoute(<any>toRaw(startingPin), pinBag);
        }
    }

    private recursiveRoute(homePin :ConnectPoint, pinBag: ConnectPoint[]) {
        homePin.part.highlight.value = HighlightType.ROUTE;
        homePin.highlight.value = HighlightType.ROUTE;
        pinBag.push(homePin);
        for (let pin of homePin.connectedTo) {
            if (!pinBag.includes(pin)){
                if (homePin.isConnectedTo(pin)) {
                    this.recursiveRoute(pin, pinBag);
                }
            }
        }
    }


}