import {ref, inject, toRaw} from 'vue'
import {Draggable, Movable, Clickable, CircuitPart} from "@/components/parts/common";
import {EditorService} from "@/components/services/EditorService";
import {BaseService} from "@/components/services/BaseService";
import {ContextMenuService} from "@/components/services/ContextMenuService";
import {CircleModel} from "@/components/parts/models";

export class MouseService extends BaseService<MouseService>() {

    /**
     * List of all dragged items - for future
     * @private
     */
    private _movingItems :Movable[] = [];

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
     * TMP var, so ondragover event is not fired multiple times when hovering on the same item
     * @private
     */
    private _lastDragOver: Draggable | null = null;

    /**
     * Get first item under cursor for drag over
     * @param mouseX
     * @param mouseY
     * @private
     */
    private getDragOverItemUnderMouse(mouseX :number, mouseY :number) :Draggable|null {
        // get list of elements
        for (let part of EditorService.inject().parts.value) {
            for (let pin of part.internalPins) {
                if (pin.draggable.isIn(mouseX, mouseY)) {
                    return <Draggable>(<any>toRaw(pin.draggable))
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


    private  get movingItem() :Movable|null {
        // return value onlu if only one item si moved
        if (this._movingItems.length == 1 && this._movingItems[0] instanceof Movable) {
            return (this._movingItems[0] as Movable);
        }
        return null;
    }
    onMouseUp(e: MouseEvent) {

        this._isDown = false;

        // on mouse release find item that is first under cursor and is tno drag start
        let dragOverTarget = this.getDragOverItemUnderMouse(e.clientX, e.clientY);

        let sourceTarget = this.movingItem;
        // got all, so clear all moving items
        this.clearRegistered();

        // next check if is possible to drag over
        if (sourceTarget != null && sourceTarget instanceof Draggable) {
            if (dragOverTarget != null) {
                // if so, hit evvents
                dragOverTarget.onDraggedOverAction(sourceTarget, dragOverTarget);
                sourceTarget.onDraggedOverAction(sourceTarget, dragOverTarget);
            }
            sourceTarget.onDraggingEndAction();
        }
    }

    onMouseMove(e: MouseEvent) {

        // if there are item to drag, then drag them
        if (this._movingItems.length > 0) {


            //else {

                // ge mouse deltas
                let deltaX = e.clientX - this._mouseX;
                let deltaY = e.clientY - this._mouseY;

                this._movingItems.forEach((m) => {
                    // do not move by mouse draggable items that cannot be dragged
                    if (m instanceof Draggable && m.canStartDrag == false) {
                        return;
                    }
                    if (m instanceof Movable) {
                        (m as Movable).moveByDelta(deltaX, deltaY);
                    }
                });
          //  }

            // move all moving items

            //store actual mouse position for next move delta
            this._mouseX = e.clientX;
            this._mouseY = e.clientY;
        }

        // if there is item directly dragged to drop
        if (this.movingItem != null && this.movingItem instanceof Draggable) {
            if (this.movingItem.canStartDrag === false) {
                return;
            }
            // get first item that is under mouse and it is not dragged
            let under = this.getDragOverItemUnderMouse(e.clientX, e.clientY);
            // check if dragged over item is not the same as last time
            if (under != null && this._lastDragOver != under) {
                // hit drag over event on not moved item
                this._lastDragOver?.onDraggingOverEndAction();
                // hit drag over event on dragged item
                under.onDraggingOverAction(under, this.movingItem);
                // store last item that is dragged over
                this._lastDragOver = under;
            }
        }
    }


    public register(items: Movable|Array<Movable>) {

        let i = []
        // if there is draggable convert it to array
        if (items instanceof Movable) {
            i.push(items);
        } else {
            i = (items as Array<Movable>);
        }

        for (let item of i) {
            // first put in moving items
            let instance = item as Movable;
            if (instance == null) continue;

            this._movingItems.push(instance);
            instance.startMovingAction();

            // check if item is draggable
            if (this.movingItem != null && this.movingItem instanceof Draggable && this.movingItem.canStartDrag === true) {
                    // if it is draggable and it is only one start dragging
                   this.movingItem.onDraggingStartAction(this.movingItem);
            }
        }
    }

    clearRegistered(){
        this._movingItems.forEach((m) => {
            (m as Movable).stopMovingAction();
            (m as Clickable).mouseReleased();
        });
        this._movingItems = [];
    }

}