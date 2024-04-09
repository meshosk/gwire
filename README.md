# The Gwire 

__IN DEVELOPMENT__

This web app will provide possibility of creating custom guitar wiring, showing active parts in circuit, coils polarity, export diagram into PDF.

___The TODO___:
- [ ] Undo/Redo
- [ ] Scheme snapshot
- [ ] PDF export
- [ ] Scheme library
- [ ] Add more parts
- [ ] Better ui design
- [ ] Create tutorial/manual wiki
- [x] Option to set wires colors
- [ ] Text/note component
- [ ] Pinpoint route validation
- [ ] Drag&Drop parts from menu

## Github pages

App is running live here [https://meshosk.github.io/gwire/](https://meshosk.github.io/gwire/)

 __Can be non or partially functional__

## How to run

Currently, there is no CI\CD pipe, to put this app on web (but planned). App can be run on locally installed node or docker. Just run it using `vite`.

If you have docker, just run from console (do not forget to run `npm install` first): 
```bash
docker run --rm -it -v ${PWD}:/mnt/app -p 80:80 -w "/mnt/app/" node:latest npm run dev
```
This should run it.