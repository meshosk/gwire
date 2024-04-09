import { ref} from "vue";


export class ContextMenuItem {

    private readonly _id;
    private static  idCount = 0;
    private static getId() {
        return ContextMenuItem.idCount++;
    }

    private _text :String;
    private _iconView :String;
    private _subMenu = [];
    private _subMenuIsOpen = ref(false);

    /**
     *
     * @param text Text shown in menu
     * @param iconView HTML of icon to be shown
     * @param subMenu ContextMenuItem[]|function Other sub menu items or function to be preformed onclick
     */
    constructor(text :String, iconView :String, subMenu:any) {
        this._id = ContextMenuItem.getId();
        this._text = text;
        this._iconView = iconView;
        this._subMenu = subMenu;
    }

    get subMenu() {
        return this._subMenu;
    }

    get text(): String {
        return this._text;
    }

    get iconView(): String {
        return this._iconView;
    }


    get subMenuIsOpen() {
        return this._subMenuIsOpen;
    }

    get id() {
        return this._id;
    }
}