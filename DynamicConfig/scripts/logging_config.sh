#!/bin/bash

configDir=$1
assemblyListFile=$2
outputFile="temp_out"
searchKeyword="DAFFODIL_TORCH_ASSEMBLY_NAME"

instConfigFile=$configDir/torch-instrumentation.torchconfig
apiLine=`grep $searchKeyword $instConfigFile` 

rm -f $outputFile
while read -r line
do
    if [[ "$line" == *"$searchKeyword"* ]]; then
        for i in `cat $assemblyListFile`
        do
           repl="${apiLine/$searchKeyword/$i}"
           echo $repl >> $outputFile
        done
    else
        echo $line >> $outputFile
    fi
done < $instConfigFile
mv $outputFile $instConfigFile
