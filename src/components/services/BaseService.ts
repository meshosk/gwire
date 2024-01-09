export abstract class BaseService<T> {

    // yeah, neat workaround, TS do not handle generics+static well
    // In TS static attribs are common for all children
    private static _services:Map<string, object> = new Map();
    constructor() {
        BaseService._services.set(this.constructor.name, this);
    }

    /**
     * Static method for easy injection
     */
    static inject()   {

       return this._services.get(this.name);
    }
}
