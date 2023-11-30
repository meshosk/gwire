import {ref} from 'vue'
 export class Movable {

     private _x = ref(0);
     private _y = ref(0);

     private _isDown = false;


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

    get isDown(): boolean {
        return this._isDown;
    }

    set isDown(value: boolean) {
        this._isDown = value;
    }

    onMouseDown() {
        this._isDown = true;
    }

     onMouseUp() {
        this._isDown = false;
    }

     onMouseMove(e: MouseEvent) {
        if (this._isDown) {
            this._x.value += e.movementX;
            this._y.value += e.movementY;
        }
    }
}