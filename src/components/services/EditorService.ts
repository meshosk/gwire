import * as modelRef from "@/components/parts/models";
import {ref, inject, toRaw} from "vue";
import  * as Vue from "@vue/reactivity"
import {CircuitPart, ConnectPoint, HighlightType} from "@/components/parts/common";
import {CircleModel, InputJackModel} from "@/components/parts/models";
import {BaseService} from "@/components/services/BaseService";

export class EditorService extends BaseService<EditorService>() {

    private readonly _parts  :Vue.Ref<CircuitPart[]>;

    private readonly _partsNormal  :Vue.Ref<CircuitPart[]> =  ref([])
    private readonly _partsPrioritized  :Vue.Ref<CircuitPart[]> =  ref([])

    constructor() {
        super();
        this._parts = ref([]);
    }
    public addPart(partName :string) : CircleModel {
        // @ts-ignore
        let instance = (new modelRef[partName]() as CircleModel);
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

       this.clearActive();

        // find input jack
        let jacks = this._parts.value.filter( x => x instanceof InputJackModel);
        let pinBag :ConnectPoint[] = [];

        for (let jack of jacks) {

            let startingPin = jack.pins.find(x => x.name == "hot");

            this.recursiveRoute(<any>toRaw(startingPin), pinBag);
        }
    }

    public clearActive() {
        for (let part of this._parts.value) {
            // @ts-ignore
            part.highlight = HighlightType.NONE; // this retuns proxy, but is not in view mode, cannot get correct types for clean build
            for (let pin of part.pins) {
                // @ts-ignore
                pin.highlight = HighlightType.NONE; // this retuns proxy, but is not in view mode, cannot get correct types for clean build
            }
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