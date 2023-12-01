import {ref, inject} from 'vue'
import {DraggableOver, Movable, Clickable} from "./basic/index";


export class MouseService {

    private _draggedItems :Object[] = [];

    private _dragSource :DraggableOver|null = null;
    private _dragTarget :DraggableOver|null = null;


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

        if (this._dragTarget != null && this._dragSource != null) {
                this._dragTarget.draggedOver(this._dragSource);
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
        this.clearDrag();

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

    registerDragSource(item : DraggableOver|null) {
        if (this._dragSource == null) {
            this._dragSource = item;
        }
    }
    registerDragTarget(item : DraggableOver|null) {
        if (this._dragSource != item) {
            this._dragTarget = item;
        }
    }


    clearDrag() {
        this._dragSource = null;
        this._dragTarget = null;
    }



    get mouseX(): number {
        return this._mouseX;
    }

    get mouseY(): number {
        return this._mouseY;
    }
}