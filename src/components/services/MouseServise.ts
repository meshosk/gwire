import {ref, inject, toRaw} from 'vue'
import {DraggableOver, Movable, Clickable} from "@/components/parts/common";
import {EditorService} from "@/components/services/EditorService";
import {BaseService} from "@/components/services/BaseService";
import {ContextMenuService} from "@/components/services/ContextMenuService";

export class MouseService extends BaseService<MouseService>() {

    /**
     * List of all dragged items - for future
     * @private
     */
    private _draggedItems :Object[] = [];

    /**
     * Item that is dragged - drag starter
     * @private
     */
    private _dragSource :DraggableOver|null = null;
    /**
     * Ref to dragged over item. This item may not be the final destination
     * @private
     */
    private _dragOverTMP :DraggableOver|null = null;

    /**
     * Previous x mouse position, for delta calculation
     * @private
     */
    private _mouseX = 0;
    /**
     * Previous y mouse position, for delta calculation
     * @private
     */
    private _mouseY = 0;

    /**
     * Mouse button is pressed down
     * @private
     */
    private _isDown = false;

    /**
     * Using shady-around-vue-play to get over which draggable component is set position. Dragged
     * item is ignored. This should be changed to use Vue API in future.
     * @param mouseX
     * @param mouseY
     * @private
     */
    private getDragUnderMouse(mouseX :number, mouseY :number) :DraggableOver|null {
        // get list of elements
        for (let part of EditorService.inject().parts.value) {
            for (let pin of part.internalPins) {
                if (pin.draggable.isIn(mouseX, mouseY)) {
                    return <DraggableOver>(<any>toRaw(pin.draggable))
                }
            }
        }
        return null;
    }
    onMouseDown(e: MouseEvent) {
        this._isDown = true;
        this._mouseX = e.clientX;
        this._mouseY = e.clientY;
        ContextMenuService.inject().closeMenu();
    }

    onMouseUp(e: MouseEvent) {
        this._isDown = false;

        let dragTarget = this.getDragUnderMouse(e.clientX, e.clientY);
        let sourceTarget = this._dragSource;

        this.clearRegistered();

        if (sourceTarget != null) {
            if (dragTarget != null) {
                dragTarget.onDraggedOverAction(sourceTarget, dragTarget);
                sourceTarget.onDraggedOverAction(sourceTarget, dragTarget);
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
                this._dragOverTMP?.onDraggingOverEndAction();
            }
            if (under != null) {
                under.onDraggingOverAction(under, this._dragSource);
                this._dragOverTMP = under;
            }
        }
    }

    register(instance: Movable) {

        if (instance instanceof DraggableOver) {
            let drg =  (instance as DraggableOver)
            if (drg.canStartDrag === false) {
                return;
            }
            this.registerDragSource(drg);
        }

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
                item.onDraggingOverAction(item, this._dragSource);
            }
        }
    }
    registerDragSource(item : DraggableOver) {
        if (!this._dragSource){
            this._dragSource = item;
            this._dragSource?.onDraggingStartAction(item);
        }
    }


    get mouseX(): number {
        return this._mouseX;
    }

    get mouseY(): number {
        return this._mouseY;
    }
}