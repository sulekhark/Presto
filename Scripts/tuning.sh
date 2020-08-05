#! /bin/bash

scale=$1
maxp=$2
minp=$3

for i in AsyncJobDispatcher  AsyncWebCrawler  FilePkgUtil  HtmlSanitizer  ScrambledSquares  WaveformGenerator; do    echo $i >> ranking_time.txt;    cat $i/bnet/bnet_time.txt >> ranking_time.txt;    echo >> ranking_time.txt;    echo >> ranking_time.txt; done

for i in AsyncJobDispatcher  AsyncWebCrawler  FilePkgUtil  HtmlSanitizer  ScrambledSquares  WaveformGenerator; do    echo $i >> find_prob_time.txt;    cat $i/bnet/find_prob_time.txt >> find_prob_time.txt;    echo >> find_prob_time.txt;    echo >> find_prob_time.txt; done

mv auc_report.txt K$scale/max_p_$maxp/min_p_$minp 
mv ranking_time.txt K$scale/max_p_$maxp/min_p_$minp 
mv find_prob_time.txt K$scale/max_p_$maxp/min_p_$minp 
