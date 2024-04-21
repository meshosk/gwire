export function BaseService<T>() {
    /**
     * Little construct to get generic access to services as easy single instance service
     */
    class BaseService {

        // yeah, neat workaround, TS do not handle generics+static well
        // In TS static attribs are common for all children
        private static _services :Map<string, object> = new Map();
        constructor() {
            BaseService._services.set(this.constructor.name, this);
        }
        /**
         * Static method for easy injection. It sucks, because Idea cannot handle right type
         * on return... need to cast in the method call place...
         */
        static inject() :T   {
            return <T>this._services.get(this.name);
        }
    }
    return BaseService;
}


