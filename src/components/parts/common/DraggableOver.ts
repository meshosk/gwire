import { ConnectPoint, Movable} from "./index";


export class DraggableOver extends Movable {

    private _dropAreaElement: any;
    private _canStartDrag = true;
    private _onDraggedOverAction: (source: DraggableOver, target: DraggableOver) => void = (source, target) => {}
    private _onDropAction: (droppedItem: DraggableOver) => void = (droppedItem :DraggableOver) => {}
    private _onDraggingOverAction: (droppedItem: DraggableOver, droppedOVer: DraggableOver) => void = (droppedItem :DraggableOver, droppedOVer: DraggableOver) => {}
    private _onDraggingOverEndAction: () => void = () => {}
    private _onDraggingStartAction: (item :DraggableOver) => void = (x) => {}
    private _onDraggingEndAction: () => void = () => {}

    private readonly _connectPoint: ConnectPoint;

    constructor( connectPoint : ConnectPoint) {
        super();
        this._connectPoint = connectPoint;
    }

    isIn(x:number, y:number):boolean{
        if (this._dropAreaElement == undefined) return false;

        let boundary = (this._dropAreaElement as HTMLElement).getBoundingClientRect()

        return x >= boundary.x  && x <= boundary.x + boundary.width
        && y >= boundary.y && y <= boundary.y + boundary.height
    }

    set dropAreaElement(value :any) {
        this._dropAreaElement = value;
    }

    get connectPoint(): ConnectPoint {
        return this._connectPoint;
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

    get onDraggingOverAction(): (droppedItem: DraggableOver, droppedOVer: DraggableOver) => void {
        return this._onDraggingOverAction;
    }

    set onDraggingOverAction(value: (droppedItem: DraggableOver, droppedOVer: DraggableOver) => void) {
        this._onDraggingOverAction = value;
    }

    get onDraggingStartAction(): (item :DraggableOver) => void {
        return this._onDraggingStartAction;
    }

    set onDraggingStartAction(value: (item :DraggableOver) => void) {
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

    get canStartDrag(): boolean {
        return this._canStartDrag;
    }

    set canStartDrag(value) {
        if ( typeof value === "string") {
            value = true === value;
        }
        this._canStartDrag = value;
    }
}