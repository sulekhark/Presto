#!/bin/bash

out_file="auc_report.txt"
rm -f $out_file
for i in AsyncJobDispatcher AsyncWebCrawler FilePkgUtil HtmlSanitizer ScrambledSquares WaveformGenerator
do
    cd $i
    rm -rf dynlogs rep*
    post_process_2.sh dynlogs.tgz
    echo $i >> ../$out_file
    cat bnet/auc.txt >> ../$out_file
    echo  >> ../$out_file
    echo  >> ../$out_file
    cd -
done
