import {ref, inject} from 'vue'
import {DraggableOver, Movable, Clickable} from "./basic/index";

export class MouseService {

    private _draggedItems :Object[] = [];

    private _dragSource :DraggableOver|null = null;
    private _dragOverTMP :DraggableOver|null = null;

    private _mouseX = 0;
    private _mouseY = 0;

    private _isDown = false;

    static inject() {
        return inject("MouseService");
    }

    private getDragUnderMouse(mouseX, mouseY) :DraggableOver|null {
        let elements = document.elementsFromPoint(mouseX, mouseY);
        for (let x of elements) {
            if (x.hasOwnProperty("__vueParentComponent")) {
                for (const property in x.__vueParentComponent.devtoolsRawSetupState) {
                    let obj = x.__vueParentComponent.devtoolsRawSetupState[property];
                    if (obj instanceof DraggableOver && obj != this._dragSource) {
                        // only draggable
                        if ((obj as DraggableOver).isIn(mouseX, mouseY)) {
                            return obj;
                        }
                    }
                }
            }
        };
        return null;
    }
    onMouseDown(e: MouseEvent) {
        this._isDown = true;
        this._mouseX = e.clientX;
        this._mouseY = e.clientY;
    }

    onMouseUp(e: MouseEvent) {
        this._isDown = false;

        let dragTarget = this.getDragUnderMouse(e.clientX, e.clientY);
        let sourceTarget = this._dragSource;

        this.clearRegistered();

        if (sourceTarget != null) {
            if (dragTarget != null) {
                dragTarget.onDraggedOverAction(sourceTarget, dragTarget);
                dragTarget.onDraggingEndAction();
            }
            sourceTarget.onDraggingEndAction();
        }

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

        if (this._dragSource != null) {
            let under = this.getDragUnderMouse(e.clientX, e.clientY);
            if (this._dragOverTMP != under) {
                this._dragOverTMP?._onDraggingOverEndAction();
            }
            if (under != null) {
                under.onDraggingOverAction(this._dragSource);
                this._dragOverTMP = under;
            }
        }
    }

    register(instance: Movable) {
        this._draggedItems.push(instance);
        instance.startMovingAction();
    }

    unregister(instance: Movable){
        let index = this._draggedItems.indexOf(instance);
        if (index>0) {
            this._draggedItems.splice(index,1);
        }
    }

    clearRegistered(){
        this._dragSource = null;
        this._draggedItems.forEach((m) => {
            if (m instanceof Movable) {
                (m as Movable).stopMovingAction();
            }

            if (m instanceof Clickable) {
                (m as Clickable).mouseReleased();
            }
        });
        this._draggedItems = [];
    }

    get isDown(): boolean {
        return this._isDown;
    }

    checkForDraggingOver(item : DraggableOver|null) {
        if (this._dragSource !== null) {
            if (item != null && item != this._dragSource) {
                item.onDraggingOverAction(this._dragSource);
            }
        }
    }
    registerDragSource(item : DraggableOver|null) {
        if (this._dragSource === null) {
            this._dragSource = item;
            this._dragSource?.onDraggingStartAction();
        } else {

        }
    }


    get mouseX(): number {
        return this._mouseX;
    }

    get mouseY(): number {
        return this._mouseY;
    }
}