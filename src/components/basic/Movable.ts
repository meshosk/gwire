import {ref} from 'vue'
import {MouseService} from "@/components/MouseServise";
import {Clickable} from "./index";


export class Movable extends Clickable {

    private _x = ref(0);
    private _y = ref(0);
    private _startMovingAction: () => void = () => {}
    private _stopMovingAction: () => void = () => {}


    /**
     * If true, current instance is actually dragged
     * @private
     */
    private _isDragged = ref(false);

    private mouseService = MouseService.inject();

    onMouseDown(e: MouseEvent) {
        this.mousePressed()
        this.mouseService.register(this);
    }

    mouseMoved(deltaX, deltaY) {
        if (this.mouseIsDown) {
            if (this._isDragged != true) {
                this._isDragged.value = true;
            }
            this._x.value += deltaX
            this._y.value += deltaY;
        } else {
            if (this._isDragged != false) {
                this._isDragged.value = false;
            }
        }
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

    set x(value) {
        this._x = value;
    }

    get y() {
        return this._y;
    }

    set y(value) {
        this._y = value;
    }

}