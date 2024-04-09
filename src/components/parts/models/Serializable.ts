export abstract class Serializable {
    public abstract get JSONObject() :object;
    public abstract setFromJSON(o :any) :void;

    /**
     * Needs to be set as string due deserialization and js compile minification
     */
    public abstract get nonMinifiedClassName() :string;
}