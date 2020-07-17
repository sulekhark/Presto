#!/bin/bash

# executes in Presto/DAFFODIL/src/test

for i in FilePkgUtil AsyncJobDispatcher WaveformGenerator AsyncWebCrawler HtmlSanitizer ScrambledSquares
do
	cd $i
	echo $PWD
	run_torch.sh dynconfig.tgz ${i}_test.txt
	cd -
done
