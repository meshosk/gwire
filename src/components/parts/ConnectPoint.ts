
export class ConnectPoint {

    private _connectedTo :ConnectPoint|null;

    public connect(connectPoint : ConnectPoint) {
        if (this != connectPoint) {
            this._connectedTo = connectPoint;
            connectPoint.connect(this);
        }
    }

    public  disconnect() {
        this._connectedTo =  null;
    }

    get isConnected(): boolean {
        return this._connectedTo != null;
    }

    get connectedTo(): ConnectPoint|null {
        return this._connectedTo;
    }
}