import * as Vue from "@vue/reactivity";
import {Color} from "@/components/parts/common/color/Color";
import type {Ref, UnwrapRef} from "vue";
import {ref} from "vue";

export class Highlight {

    private readonly colors;
    private readonly highlightColor :Color = new Color("#FF0000");
    private readonly resultColor : Ref<UnwrapRef<Color>>;

    private isHighlighted = ref(false);

    constructor() {
        this.colors = ref([]);
        this.resultColor =  Vue.ref(new Color("#000000"));
    }

    private recalculateColor(){

        // if (this.isHighlighted.value) {
        //     this.resultColor.value = this.highlightColor;
        //     return;
        // }
        //
        // let r = null;
        // for (let color of this.colors.value) {
        //     if (r == null) {
        //         r = color;
        //     } else {
        //         r = r.mix(color, .5);
        //     }
        // }
        // this.resultColor.value = r;
    }

    public addColorToStack(color :Color) {
      //   this.colors.value.push(color);
    }

}