import {ConnectPoint} from "@/components/parts/ConnectPoint";

export class Cable {

    private pointA :ConnectPoint = new ConnectPoint();
    private pointB :ConnectPoint = new ConnectPoint();

    constructor() {
        this.pointA.connect(this.pointB);
    }
}