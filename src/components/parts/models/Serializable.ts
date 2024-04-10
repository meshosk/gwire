export abstract class Serializable {
    /**
     * Get data needed for serialization
     */
    public abstract get JSONObject() :object;
    /**
     *
     * @param o {any} JSON object with data that will be pushed into existing instance
     */
    public abstract setFromJSON(o :any) :void;
    /**
     * Needs to be set as string due deserialization and js compile minification
     */
    public abstract get nonMinifiedClassName() :string;
}