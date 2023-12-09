import {watch, inject} from "vue";
import type {DraggableOver} from "@/components/parts/common/DraggableOver";

export class ConnectionLockService {

    private readonly _watchers: Map<MapKey, WatcherItem> = new Map<MapKey, WatcherItem>();

    static inject() :ConnectionLockService  {
        return <ConnectionLockService>inject("ConnectionLockService");
    }

    lock( a: DraggableOver, b: DraggableOver, propsFunction, changeFunction, releaseAction) {
        let key = this.findWatcherKey( a, b);
        if (key == null) {
            let watcher = new WatcherItem(propsFunction, changeFunction,releaseAction )
            this._watchers.set(new MapKey(a,b), watcher);
            return watcher;
        } else {
            return key;
        }
    }

    private findWatcherKey(a: DraggableOver, b: DraggableOver){
        for (let key :MapKey of this._watchers.keys()) {
            if (key.is(a,b)) {
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
            if (key.has(a)) {
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

    public has(a :DraggableOver){
        return this.a == a || this.b == a;
    }
    public is(aa :DraggableOver,bb :DraggableOver) :boolean {
        return this.a == aa && this.b == bb || this.a == bb && this.b == aa;
    }
}

class WatcherItem {
    private _watcher;
    private _releaseAction;

    constructor(propsFunction, changeFunction, releaseAction) {
        this._watcher = watch(propsFunction, changeFunction );
        this._releaseAction = releaseAction;
    }

    public releaseWatcher(){
        this._watcher();
        this._releaseAction();
    }
}