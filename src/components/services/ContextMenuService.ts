import {BaseService} from "./BaseService";
import {ref} from "vue";
export class ContextMenuService extends BaseService<ContextMenuService>() {

    private _menuIsOpen = ref(false);
    private _menuItems = Array;

    private _x = ref(0);
    private _y = ref(0);

    public get menuIsOpen() {
        return this._menuIsOpen;
    }

    public openMenu(x :number, y :number, menuItems :any) {
        this._menuItems = menuItems;
        this._menuIsOpen.value = true;
        this._x.value = x;
        this._y.value = y;
    }

    public closeMenu() {
        this._menuIsOpen.value = false;
        this._menuItems = [];
    }

    get menuItems() {
        return this._menuItems;
    }

    get x(){
        return this._x;
    }

    get y() {
        return this._y;
    }
}