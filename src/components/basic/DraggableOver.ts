import {Movable} from "./index";

export class DraggableOver extends Movable {

    mouseIsOver(){
       this.mouseService.registerDraggableOver(this);
    }

    mouseIsOut(){
       this.mouseService.unRegisterDraggableOver(this);
    }
    draggedTo(itemsDraggedOver: DraggableOver[]){
        console.log("pop");
    }
}