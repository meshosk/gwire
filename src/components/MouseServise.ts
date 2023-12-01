import {ref, inject} from 'vue'
import {Movable} from "@/components/basic/Movable";
import {Clickable} from "@/components/basic/Clickable";
import {DraggableOver} from "@/components/basic/DraggableOver";
export class MouseService {

    private _draggedItems :Object[] = [];
    private _draggableOver :DraggableOver|null = null;
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

        if (this._draggableOver != null) {
            let draggables =  this._draggedItems.filter( (i:Movable) => {
                return i instanceof DraggableOver;
            }).map( (i:Movable) => (i as DraggableOver));
            this._draggableOver.draggedTo(draggables);
        }


        this.clearRegistered();
    }

    onMouseMove(e: MouseEvent) {
        if (this._isDown) {

            let deltaX = e.clientX - this._mouseX;
            let deltaY = e.clientY - this._mouseY;

            this._draggedItems.forEach((m) => {
                if (m instanceof Movable) {
                    (m as Movable).mouseMoved(deltaX, deltaY);
                }
            });

            this._mouseX = e.clientX;
            this._mouseY = e.clientY;
        }
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
            if (m instanceof Clickable) {
                m.mouseReleased();
            }
        });
        this._draggedItems = [];
    }

    get isDown(): boolean {
        return this._isDown;
    }

    registerDraggableOver(item : DraggableOver) {
        this._draggableOver = item;
    }
    unRegisterDraggableOver(item : DraggableOver) {
        if (this._draggableOver == item) {
            this._draggableOver = null;
        }
    }



    get mouseX(): number {
        return this._mouseX;
    }

    get mouseY(): number {
        return this._mouseY;
    }
}