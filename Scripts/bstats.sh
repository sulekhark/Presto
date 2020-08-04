#!/bin/bash

outFile="bstats.txt"
rm -f $outFile
for i in AsyncJobDispatcher AsyncWebCrawler FilePkgUtil HtmlSanitizer ScrambledSquares WaveformGenerator
do
    ntc=`cat $i/datalog/classes.txt | wc -l`
    nac=`grep -v "CLASS:System\." $i/datalog/classes.txt | grep -v "CLASS:Daffodil\." | grep -v "CLASS:Microsoft\." | grep -v "MODULE:mscorlib" | wc -l`

    ntm=`cat $i/datalog/methods.txt | wc -l`
    nam=`grep -v "CLASS:System\." $i/datalog/methods.txt | grep -v "CLASS:Daffodil\." | grep -v "CLASS:Microsoft\." | grep -v "EmptyArray" | wc -l`    

    echo $i >> $outFile
    echo "Total classes:" $ntc  "App classes:" $nac  "Total meths:" $ntm  "App meths:" $nam >> $outFile
    head -n 2 $i/datalog/factgen_log.txt >> $outFile
    echo >> $outFile
    echo >> $outFile
done
