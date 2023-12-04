# The Gwire 

__IN DEVELOPMENT__

This web app will provide possibility of creating custom guitar wiring, showing active parts in circuit, coils polarity, export diagram into PDF.

## How to run

Currently, there is no CI\CD pipe, to put this app on web (but planned). App can be run on locally installed node or docker. Just run it using `vite`.

If you have docker, just run from console (do not forget to run `npm install` first): 
```bash
docker run --rm -it -v ${PWD}:/mnt/app -p 80:80 -w "/mnt/app/" node:latest npm run dev
```
This should run it.