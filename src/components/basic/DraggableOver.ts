import {Movable} from "./index";

export class DraggableOver extends Movable {

    setAsSource(){
       this.mouseService.registerDragSource(this);
    }

    setAsTarget(){
        this.mouseService.registerDragTarget(this);
    }

    clearTarget() {
        this.mouseService.registerDragTarget(null);
    }

    stopDragging(){
       this.mouseService.clearDrag();
    }
    draggedOver(itemsDraggedOver: DraggableOver){
        console.log("pop");
    }
}