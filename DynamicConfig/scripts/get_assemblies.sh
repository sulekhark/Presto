#!/bin/bash

modulesFile=$1
outputFile=$2
blacklistFile="$PRESTO_HOME/DynamicConfig/scripts/blacklist.txt"

rm -f $outputFile
while read -r line
do
    fname=`echo $line | awk -F ':' '{print $4;}'`
    asmName=`echo $fname | awk -F '\' '{print $NF;}'`
    exclude=0
    while read -r pre
    do
        if [[ $asmName == $pre* ]]
        then
            exclude=1
        fi
    done < $blacklistFile
    if [ $exclude -eq 0 ]
    then
        echo $asmName >> $outputFile
    fi
done < $modulesFile
