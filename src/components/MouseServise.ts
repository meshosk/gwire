import {ref, inject} from 'vue'
import type {Movable} from "@/components/basic/Movable";
export class MouseService {

    private _draggedItems :Movable[] = [];

    private _mouseX = 0;
    private _mouseY = 0;

    private _isDown = false;

    static inject() {
        return inject("MouseService");
    }

    onMouseDown(e: MouseEvent) {
        this._isDown = true;
        this._mouseX = e.clientX;
        this._mouseY = e.clientY;
    }

    onMouseUp() {
        this._isDown = false;
        this.clearRegistered();
    }

    onMouseMove(e: MouseEvent) {
        if (this._isDown) {

            let deltaX = e.clientX - this._mouseX;
            let deltaY = e.clientY - this._mouseY;

            this._draggedItems.forEach((m) => {
                m.mouseMoved(deltaX, deltaY);
            });

            this._mouseX = e.clientX;
            this._mouseY = e.clientY;
        }
        console.log("MS move")
    }

    register(instance: Movable) {
        this._draggedItems.push(instance);
    }

    unregister(instance: Movable){
        let index = this._draggedItems.indexOf(instance);
        if (index>0) {
            this._draggedItems.splice(index,1);
        }
    }

    clearRegistered(){
        this._draggedItems.forEach((m) => {
            m.isDragged.value = false;
        });
        this._draggedItems = [];
    }

    get isDown(): boolean {
        return this._isDown;
    }

    get mouseX(): number {
        return this._mouseX;
    }

    get mouseY(): number {
        return this._mouseY;
    }
}