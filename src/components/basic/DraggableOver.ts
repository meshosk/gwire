import {Movable} from "@/components/basic/Movable";

export class DraggableOver extends Movable {

    mouseIsOver(){
        this.mouseService.registerDraggableOver(this);
    }

    mouseIsOut(){
        this.mouseService.unRegisterDraggableOver(this);
    }
    draggedTo(itemsDraggedOver: DraggableOver[]){

    }
}