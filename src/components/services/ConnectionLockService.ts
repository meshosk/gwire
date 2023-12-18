import {watch, inject} from "vue";
import type {DraggableOver} from "@/components/parts/common/DraggableOver";
import {DraggableOver} from "@/components/parts/common/DraggableOver";

export class ConnectionLockService {

    private readonly _watchers: Map<MapKey, WatcherItem> = new Map<MapKey, WatcherItem>();

    private static _service = null;
    /**
     * Static method for easy injection
     */
    static inject() : ConnectionLockService {
        return this._service;
    }


    constructor() {
        ConnectionLockService._service = this;
    }

    lock( a: DraggableOver, b: DraggableOver) {
        let key = this.findWatcherKey( a, b);
        if (key == null) {
            let watcher = new WatcherItem(a,b);
            this._watchers.set(new MapKey(a,b), watcher);
            return watcher;
        } else {
            return key;
        }
    }

    private findWatcherKey(a: DraggableOver, b: DraggableOver){
        for (let key :MapKey of this._watchers.keys()) {
            if (key.has(a, b)) {
                return key;
            }
        }
    }

    public releaseLock(a: DraggableOver, b: DraggableOver) {
        let key =  this.findWatcherKey(a,b);
        if (key != null) {
            let watcher = this._watchers.get(key);
            this._watchers.delete(key);
            watcher.releaseWatcher();
        }
    }

    public releaseAllLockFor(a: DraggableOver){
        let keys = [];
        for (let key :MapKey of this._watchers.keys()) {
            if (key.isInKey(a)) {
                keys.push(key);
            }
        }

        for (let key :MapKey of keys) {
            let watcher = this._watchers.get(key);
            this._watchers.delete(key);
            watcher.releaseWatcher();
        }
    }

    public get JSONObject() {
        let res = [];
        for (let item :WatcherItem of this._watchers.values()) {
            res.push(item.JSONObject);
        }
        return {
            type: this.constructor.name,
            locks: res
        };
    }

}

class MapKey {
    private readonly a :DraggableOver;
    private readonly b :DraggableOver;
    constructor(a: DraggableOver, b: DraggableOver) {
        this.a = a;
        this.b = b;
    }

    public has(a :DraggableOver, b :DraggableOver) :boolean{
        return this.a == a && this.b == b || this.a == b && this.b == a;
    }

    public isInKey(a :DraggableOver){
        return this.a == a || this.b == a;
    }
}

class WatcherItem {
    private readonly _watcher;
    private readonly _releaseAction;
    private readonly _a: DraggableOver;
    private readonly _b: DraggableOver;
    private _isActive = true;

    constructor(a :DraggableOver, b :DraggableOver) {
        this._a = a;
        this._b = b;
        a.connectPoint.connect(b.connectPoint);
        // only cables are the ones that can create connection
        // if this change, there will must be created watcher on a draggable, causing problem with shifted position
        // to non-shifted conversion
       this._watcher = watch(() => [b.x.value, b.y.value], (newX) => {
           if (a.x.value != b.xShifted.value) {
               a.x.value = b.xShifted.value
           }
           if (a.y.value != b.yShifted.value) {
               a.y.value = b.yShifted.value;
           }
       });
       this._releaseAction = () => {
           this._watcher();
           a.connectPoint.disconnect(b.connectPoint);
           this._isActive = false;
       }
    }

    public releaseWatcher(){
        this._releaseAction();
    }


    get a(): DraggableOver {
        return this._a;
    }

    get b(): DraggableOver {
        return this._b;
    }

    public get JSONObject() {
        return {
            a: {
                point : this._a.connectPoint.name,
                part: this._a.connectPoint.part.id
            },
            b :{
                point : this._b.connectPoint.name,
                part: this._b.connectPoint.part.id
            }
        }
    }
}