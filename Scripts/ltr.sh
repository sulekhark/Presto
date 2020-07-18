#!/bin/bash

outFile="last_true_rank.txt"
rm -f $outFile
for i in AsyncJobDispatcher AsyncWebCrawler FilePkgUtil HtmlSanitizer ScrambledSquares WaveformGenerator
do
   echo $i >> $outFile
   grep "TrueAlarm" $i/bnet/alarm_ranking.txt | tail -n 1 >> $outFile
   echo >> $outFile
done
