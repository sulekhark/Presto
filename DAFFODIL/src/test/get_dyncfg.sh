#!/bin/bash

# executes in Presto/DAFFODIL/src/test

for i in FilePkgUtil AsyncJobDispatcher WaveformGenerator AsyncWebCrawler HtmlSanitizer ScrambledSquares
do
	cd $i
	echo $PWD
	getLinux.sh $i dynconfig.tgz
	cd -
done
