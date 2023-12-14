<script setup lang="ts">
import ConnectorView from "@/components/parts/views/ConnectorView.vue";
import {DraggableOver, Movable} from "@/components/parts/common";
import {InputJackModel} from "@/components/parts/models";
import Connector from "@/components/parts/views/ConnectorView.vue";
import {toRaw} from "vue";

const props = defineProps(['model'])
const model :InputJackModel = toRaw(props.model); // wrapped in proxy


function onDragOver(source :DraggableOver, target :DraggableOver) {
  source.x.value = target.xShifted.value;
  source.y.value = target.yShifted.value;
}
</script>

<template>
  <g :transform="`translate(${model.m.x.value} ${model.m.y.value - 10})`" @mousedown="model.m.onMouseDown">
    <g
        id="layer1">
      <rect
          style="fill:#cccccc;stroke-width:0.683408"
          id="rect443"
          width="16.342659"
          height="54.733543"
          x="39.55389"
          y="4.2294302" />
      <rect
          style="fill:#808080;stroke-width:0.683408"
          id="rect227"
          width="29.818182"
          height="27.731663"
          x="32.768341"
          y="60.422539" />
      <rect
          style="fill:#cccccc;stroke-width:0.683408"
          id="rect225"
          width="56.67366"
          height="9.6695919"
          x="19.292814"
          y="64.618774" />
    </g>
    <rect
        style="fill:#000000;stroke-width:0.683408"
        id="rect223"
        width="89.789047"
        height="10.125706"
        x="1.7555168"
        y="58.005135" />
    <rect
        style="fill:#cccccc;stroke-width:0.691594"
        id="rect447"
        width="11.850817"
        height="31.015676"
        x="75.966476"
        y="33.876766" />
    <rect
        style="fill:#cccccc;stroke-width:0.683408"
        id="rect447-3"
        width="11.850817"
        height="30.285894"
        x="6.0084286"
        y="27.67363" />
    <g
        id="g612"
        transform="matrix(1.6178551,0,0,2.3330631,38.036872,9.7156951)">
      <rect
          style="fill:#000000;stroke-width:0.264583"
          id="rect524"
          width="4.8439631"
          height="0.45186219"
          x="24.743975"
          y="17.676849" />
      <rect
          style="fill:#000000;stroke-width:0.195954"
          id="rect524-8"
          width="3.1630356"
          height="0.37956423"
          x="25.530216"
          y="18.336571" />
      <rect
          style="fill:#000000;stroke-width:0.164567"
          id="rect524-8-2"
          width="2.6027265"
          height="0.32534075"
          x="25.855555"
          y="19.023399" />
      <rect
          style="fill:#000000;stroke-width:0.125815"
          id="rect524-8-5"
          width="1.8255235"
          height="0.2711173"
          x="26.180897"
          y="19.67408" />
      <rect
          style="fill:#000000;stroke-width:0.125815"
          id="rect524-8-5-3"
          width="1.8255235"
          height="0.2711173"
          x="16.113407"
          y="-27.211143"
          transform="rotate(90)" />
    </g>
  </g>
  <ConnectorView
      :x="model.m.x.value" :x-shift="12"
      :y="model.m.y.value" :y-shift="15"
      is-draggable="false"
      :onDraggedOver="onDragOver"
      :connection="model.pins[0]"
  />
  <ConnectorView
      :x="model.m.x.value" :x-shift="85"
      :y="model.m.y.value" :y-shift="15"
      is-draggable="false"
      :onDraggedOver="onDragOver"
      :connection="model.pins[1]"
  />

</template>