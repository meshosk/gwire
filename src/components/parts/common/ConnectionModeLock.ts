import {watch} from "vue";
import type {DraggableOver} from "@/components/parts/common/DraggableOver";

export class ConnectionModeLock {

    private readonly _watchers: Map<MapKey, WatcherItem> = new Map<MapKey, WatcherItem>();

    lock( a: DraggableOver, b: DraggableOver, propsFunction, changeFunction, releaseAction) {
        if (this.findWatcher( a, b) == null) {
            this._watchers.set(new MapKey(a,b), new WatcherItem(propsFunction, changeFunction,releaseAction ))
        }
    }


    private findWatcher(a: DraggableOver, b: DraggableOver){
        for (let key :MapKey of this._watchers.keys()) {
            if (key.is(a,b)) {
                return key;
            }
        }
    }

    public releaseLock(a: DraggableOver, b: DraggableOver) {
        let key =  this.findWatcher(a,b);
        if (key != null) {
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

    public is(aa,bb) :boolean {
        return this.a == aa && this.b == bb;
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