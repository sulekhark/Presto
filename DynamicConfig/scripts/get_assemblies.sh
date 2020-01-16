#!/bin/bash

modulesFile=$1
outputFile=$2

rm -f $outputFile
while read -r line
do
    fname=`echo $line | awk -F ':' '{print $4;}'`
    asmName=`echo $fname | awk -F '\' '{print $NF;}'`
    echo $asmName >> $outputFile
done < $modulesFile
