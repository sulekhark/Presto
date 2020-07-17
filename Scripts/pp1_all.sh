#!/bin/bash

for i in AsyncJobDispatcher AsyncWebCrawler FilePkgUtil HtmlSanitizer ScrambledSquares WaveformGenerator
do
    cd $i
    echo $PWD
    post_process_1.sh datalog.tgz
    echo "=================================================="
    echo
    echo
    cd -
done
