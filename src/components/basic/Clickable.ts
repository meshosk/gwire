import {ref} from "vue";
export class Clickable {

    private _mouseIsDown = ref(false);

    get mouseIsDown() {
        return this._mouseIsDown;
    }

    mousePressed(){
        this._mouseIsDown.value = true;
    }

    mouseReleased() {
        this._mouseIsDown.value = false;
    }
}