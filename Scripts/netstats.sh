#!/bin/bash

outFile="netstats.txt"
rm -f $outFile
for i in AsyncJobDispatcher AsyncWebCrawler FilePkgUtil HtmlSanitizer ScrambledSquares WaveformGenerator
do
    s_num_cl=`cat $i/bnet/named_cons_all.txt | wc -l`
    cd $i
    s_num_tup=`node_count.sh`
    cd -
    echo $i >> $outFile
    echo "num clauses after SA:" $s_num_cl "num tuples after SA:" $s_num_tup >> $outFile
    head -n 2 $i/bnet/cons_all2bnet.log >> $outFile
    dyn_tups=`find $i/dynconfig -name torch-instr\* | wc -l`
    echo "num of dyn tups:" $dyn_tups  >> $outFile
    dyn_runs=`find $i/dynlogs -name \*.decompressed | wc -l`
    echo "num of dyn runs:" $dyn_runs  >> $outFile
    echo >> $outFile
done
