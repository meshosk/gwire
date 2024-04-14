import {watch, inject} from "vue";
import type {Draggable} from "@/components/parts/common/Draggable";
import {BaseService} from "@/components/services/BaseService";

export class ConnectionLockService extends BaseService<ConnectionLockService>() {

    private readonly _watchers: Map<MapKey, WatcherItem> = new Map<MapKey, WatcherItem>();

    lock(a: Draggable, b: Draggable) {
        this.cleanInactiveLocks();
        let key = this.findWatcherKey( a, b);
        if (key == null) {
            let watcher = new WatcherItem(a,b);
            this._watchers.set(new MapKey(a,b), watcher);
            return watcher;
        } else {
            return key;
        }
    }

    private findWatcherKey(a: Draggable, b: Draggable){
        for (let key  of this._watchers.keys()) {
            if (key.has(a, b)) {
                return key;
            }
        }
    }

    public releaseLock(a: Draggable, b: Draggable) {
        let key =  this.findWatcherKey(a,b);
        if (key != null) {
            let watcher = this._watchers.get(key);
            this._watchers.delete(key);
            watcher?.releaseWatcher();
        }
    }

    public releaseAllLock(){
        for (let lock  of this._watchers.values()){
            lock.releaseWatcher()
        }
        this._watchers.clear();
    }

    public releaseAllLockFor(a: Draggable){
        let keys = [];
        for (let key  of this._watchers.keys()) {
            if (key.isInKey(a)) {
                keys.push(key);
            }
        }

        for (let key of keys) {
            let watcher = this._watchers.get(key);
            this._watchers.delete(key);
            watcher?.releaseWatcher();
        }
    }

    public cleanInactiveLocks() {
        let copy = Array.from(this._watchers.keys());
        for (let key of copy) {
            let w = this._watchers.get(key);
            if (w?.isActive == false) {
                this._watchers.delete(key);
            }
        }
    }
}

class MapKey {
    private readonly a :Draggable;
    private readonly b :Draggable;
    constructor(a: Draggable, b: Draggable) {
        this.a = a;
        this.b = b;
    }

    public has(a :Draggable, b :Draggable) :boolean{
        return this.a == a && this.b == b || this.a == b && this.b == a;
    }

    public isInKey(a :Draggable){
        return this.a == a || this.b == a;
    }
}

class WatcherItem {
    private readonly _watcher;
    private readonly _releaseAction;
    private readonly _a: Draggable;
    private readonly _b: Draggable;
    private _isActive = true;

    constructor(a :Draggable, b :Draggable) {
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

    get isActive(): boolean {
        return this._isActive;
    }

    get a(): Draggable {
        return this._a;
    }

    get b(): Draggable {
        return this._b;
    }
}