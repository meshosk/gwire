/**
 * Simple class presenting color with extra methods
 */
export class Color {

    private readonly _color;

    constructor(color: string) {
        this._color = color;
    }

    get color(): string {
        return this._color;
    }

    public mix(color :Color,  currentColorPercentage: number) :Color{
        let mixed = this.mixColors(this.color, color.color, currentColorPercentage);
        return new Color(mixed);
    }

    /**
     * Mix two color in hex based on percentage. ChatGPT generated.
     * @param color1
     * @param color2
     * @param percentage
     * @private
     */
    private mixColors(color1 :string, color2:string, percentage: number) :string {
        // Convert hexadecimal color strings to RGB
        let rgb1 = this.hexToRgb(color1);
        let rgb2 = this.hexToRgb(color2);

        // Calculate mixed RGB values
        let mixedR = Math.round((1 - percentage) * rgb1.r + percentage * rgb2.r);
        let mixedG = Math.round((1 - percentage) * rgb1.g + percentage * rgb2.g);
        let mixedB = Math.round((1 - percentage) * rgb1.b + percentage * rgb2.b);

        // Convert mixed RGB values back to hexadecimal color string
        let mixedColor = this.rgbToHex(new RGBColor(mixedR, mixedG, mixedB));

        return mixedColor;
    }

    /**
     * Turn hex color into RGB object
     * @param hex
     * @private
     */
     private hexToRgb(hex :string) :RGBColor {
        // Remove '#' if present
        hex = hex.replace('#', '');

        // Convert hexadecimal to RGB
        let r = parseInt(hex.substring(0, 2), 16);
        let g = parseInt(hex.substring(2, 4), 16);
        let b = parseInt(hex.substring(4, 6), 16);

        return new RGBColor(r,g,b);
    }

    /**
     * Turn RGB object into hex string color
     * @param r
     * @param g
     * @param b
     * @private
     */
    private rgbToHex(rgbColor :RGBColor) {
        // Convert RGB to hexadecimal
        let componentToHex = (c :number) => {
            const hex = c.toString(16);
            return hex.length == 1 ? '0' + hex : hex;
        };
        return '#' + componentToHex(rgbColor.r) + componentToHex(rgbColor.g) + componentToHex(rgbColor.b);
    }
}

 class RGBColor {

     constructor(r: number, g: number, b: number) {
         this.r = r;
         this.g = g;
         this.b = b;
     }

     public r : number = 0;
    public g : number = 0;
    public b : number = 0;
}