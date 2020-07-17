#!/bin/bash

# executes in Presto/DAFFODIL/src/test

for i in FilePkgUtil AsyncJobDispatcher WaveformGenerator AsyncWebCrawler HtmlSanitizer ScrambledSquares
do
	cd $i
	echo $PWD
	putLinux.sh $i dynlogs.tgz
	cd -
done
