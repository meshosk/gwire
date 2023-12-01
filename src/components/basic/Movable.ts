import {ref} from 'vue'
import {MouseService} from "@/components/MouseServise";
import {Clickable} from "./index";


export class Movable extends Clickable {

    private _x = ref(0);
    private _y = ref(0);

    /**
     * If true, current instance is actually dragged
     * @private
     */
    private _isDragged = ref(false);

    public mouseService = MouseService.inject();

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