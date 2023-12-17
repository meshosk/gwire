import {watch, inject} from "vue";
import type {DraggableOver} from "@/components/parts/common/DraggableOver";

export class ConnectionLockService {

    private readonly _watchers: Map<MapKey, WatcherItem> = new Map<MapKey, WatcherItem>();

    static inject() :ConnectionLockService  {
        return <ConnectionLockService>inject("ConnectionLockService");
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
    constructor(a :DraggableOver, b :DraggableOver) {
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
       }
    }

    public releaseWatcher(){
        this._releaseAction();
    }
}