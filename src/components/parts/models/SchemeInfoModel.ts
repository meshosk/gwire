import {Serializable} from "@/components/parts/models/Serializable";

export class SchemeInfoModel extends Serializable {

    private author :string  = "";
    private schemeName :string  = "";
    private dateCreated  :Date  = new Date();
    private description :string  = "";

    get JSONObject(): object {
        return {
            'author' : this.author,
            'schemeName' : this.schemeName,
            'dateCreated' : this.dateCreated.toISOString(),
            'description' : this.description,

        };
    }

    setFromJSON(o: any) {
        if(this.nonMinifiedClassName != o.type) {
            throw new Error("Wrong JSON object");
        }

        this.author = o.author;
        this.schemeName = o.schemeName;
        this.dateCreated = new Date(o.dateCreated);
        this.description = o.description;
    }

    get nonMinifiedClassName(): string {
        return "SchemeInfoModel";
    }
}