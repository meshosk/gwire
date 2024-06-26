import {computed, ref} from 'vue'

import {MouseService} from "@/components/services/MouseServise";
import {Clickable} from "./index";
import * as Vue from "@vue/reactivity";

export class Movable extends Clickable {

    private _x = ref(0);
    private _y = ref(0);
    private _xShift = ref(0);
    private _yShift = ref(0);

    private _xShifted;
    private _yShifted;

    private _startMovingAction: () => void = () => {}
    private _stopMovingAction: () => void = () => {}

     /**
     * If true, current instance is actually dragged
     * @private
     */
    private _isDragged = ref(false);

    private _mouseService: MouseService;


    constructor() {
        super();
        this._mouseService =  (MouseService.inject() as MouseService);
        // computed need to be initialized in constructor
        this._xShifted = computed(() => {
            return this._x.value + this._xShift.value;
        });
        this._yShifted = computed(() => {
            return this._y.value + this._yShift.value;
        });

    }

    onMouseDown(e: MouseEvent|null) {
        this.mousePressed()
        this.mouseService.register(this);
    }

    moveByDelta(deltaX :number, deltaY :number) {
        if (this.mouseIsDown.value) {
            if (this.isDragged.value != true) {
                this.isDragged.value = true;
            }
            this.x.value += deltaX
            this.y.value += deltaY;
        } else {
            if (this.isDragged.value != false) {
                this.isDragged.value = false;
            }
        }
    }

    get xShifted() {
        return this._xShifted;
    }

    get yShifted() {
        return this._yShifted;
    }

    get xShift() {
        return this._xShift;
    }
    get yShift(){
        return this._yShift;
    }

    get startMovingAction(): () => void {
        return this._startMovingAction;
    }

    set startMovingAction(value: () => void) {
        this._startMovingAction = value;
    }

    get stopMovingAction(): () => void {
        return this._stopMovingAction;
    }

    set stopMovingAction(value: () => void) {
        this._stopMovingAction = value;
    }

    get isDragged() {
        return this._isDragged;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }

    get mouseService(): MouseService {
        return this._mouseService;
    }

    public get JSONObject() {
        return {
            x: this.x.value,
            y:  this.y.value,
            xShift: this.xShift.value,
            yShift: this.yShift.value
        }
    }
    public setFromJSON(o :any) {
       this.x.value = o.x;
       this.y.value = o.y;
       this.xShift.value = o.xShift;
       this.yShift.value = o.yShift;
    }

    public get XYString() {
        if (Vue.isProxy(this)) {
            return this.x + "," + this.y;
        }
        return this.x.value + "," + this.y.value;
    }
}