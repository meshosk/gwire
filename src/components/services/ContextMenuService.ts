import {BaseService} from "./BaseService";
import {ref} from "vue";
export class ContextMenuService extends BaseService<ContextMenuService>() {

    private _menuIsOpen = ref(false);
    private _menuItems = ref([]);

    private _x = ref(0);
    private _y = ref(0);

    public get menuIsOpen() {
        return this._menuIsOpen;
    }

    public openMenu(x :number, y :number, menuItems :any) {
        this.hideSubMenu(menuItems);
        this._menuItems.value = menuItems;
        this._menuIsOpen.value = true;
        this._x.value = x;
        this._y.value = y;
    }

    /**
     *
     * @param items hides menu items recursively
     * @private
     */
    private  hideSubMenu(items:any){
        for (const item of items) {
            item.subMenuIsOpen.value = false
            if (item.subMenu && Array.isArray(item.subMenu) == true) {
                this.hideSubMenu(item.subMenu);
            }
        }
    }

    public closeMenu() {
        this._menuIsOpen.value = false;
        this._menuItems.value =[];
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