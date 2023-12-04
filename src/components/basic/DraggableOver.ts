import {Movable} from "./index";
import type Ref from "vue";
import {MouseService} from "@/components/MouseServise";

export class DraggableOver extends Movable {

    private _dropAreaElement: Ref<HTMLElement | undefined>;
    private _mouseService = MouseService.inject();
    private _mainVueComponent: any;
    private _onDraggedOverAction: (source: DraggableOver, target: DraggableOver) => void = (source, target) => {
        target.x.value = source.x.value;
        target.y.value = source.y.value;
    }

    private _onDropAction: (droppedItem: DraggableOver) => void = (droppedItem :DraggableOver) => {}
    private _onDraggingOverAction: (droppedItem: DraggableOver) => void = (droppedItem :DraggableOver) => {}
    private _onDraggingOverEndAction: () => void = () => {}
    private _onDraggingStartAction: () => void = () => {}
    private _onDraggingEndAction: () => void = () => {}



    constructor( dropAreaElement :Ref<HTMLElement | undefined>) {
        super();
        this._dropAreaElement = dropAreaElement;
    }

    isIn(x:number, y:number):boolean{
        if (this._dropAreaElement.value == undefined) return false;

        let boundary = (this._dropAreaElement.value as HTMLElement).getBoundingClientRect()

        return x >= boundary.x  && x <= boundary.x + boundary.width
        && y >= boundary.y && y <= boundary.y + boundary.height
    }


    get mainVueComponent(): any {
        return this._mainVueComponent;
    }

    get onDraggingEndAction(): () => void {
        return this._onDraggingEndAction;
    }

    set onDraggingEndAction(value: () => void) {
        this._onDraggingEndAction = value;
    }

    get onDraggingOverEndAction(): () => void {
        return this._onDraggingOverEndAction;
    }

    set onDraggingOverEndAction(value: () => void) {
        this._onDraggingOverEndAction = value;
    }

    get onDraggingOverAction(): (droppedItem: DraggableOver) => void {
        return this._onDraggingOverAction;
    }

    set onDraggingOverAction(value: (droppedItem: DraggableOver) => void) {
        this._onDraggingOverAction = value;
    }

    get onDraggingStartAction(): () => void {
        return this._onDraggingStartAction;
    }

    set onDraggingStartAction(value: () => void) {
        this._onDraggingStartAction = value;
    }

    get onDraggedOverAction(): (source: DraggableOver, target: DraggableOver) => void {
        return this._onDraggedOverAction;
    }

    set onDraggedOverAction(value: (source: DraggableOver, target: DraggableOver)  => void) {
        this._onDraggedOverAction = value;
    }

    set onDropAction(value: (droppedItem: DraggableOver) => void) {
        this._onDropAction = value;
    }


    setAsSource(){
       this._mouseService.registerDragSource(this);
    }

    stopDragging(){
       this._mouseService.clearDrag();
    }
}