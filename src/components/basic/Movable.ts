import {ref} from 'vue'
import {MouseService} from "@/components/MouseServise";
 export class Movable {

     private _x = ref(0);
     private _y = ref(0);

     private _isDragged = ref(false);

     private _canTrack = false;

     private mouseService = MouseService.inject();

    onMouseDown(e: MouseEvent) {
        this._canTrack = true;
        this.mouseService.register(this);
    }

    mouseMoved(deltaX, deltaY){
        if (this._canTrack) {
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

     set isDragged(value) {
         this._isDragged = value;
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