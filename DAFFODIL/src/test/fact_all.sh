#!/bin/bash

# executes in Presto/DAFFODIL/src/test

for i in FilePkgUtil AsyncJobDispatcher WaveformGenerator AsyncWebCrawler HtmlSanitizer ScrambledSquares
do
	cd $i
	echo $PWD
	/c/Users/sulek/work/ExcAnalysis/Presto/DAFFODIL/src/app/Daffodil.FactGeneratorSA/bin/Debug/Daffodil.FactGeneratorSA.exe daffodil.cfg
	cd -
done
