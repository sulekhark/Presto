#!/bin/bash

data_dir=$1

cd $data_dir
for i in $DAFFODIL_HOME/Datalog/*
do
    cp $i .
    fname=`basename $i`
    dos2unix $fname
done
cp ../logs/rta_log.txt .
cp ../logs/tac_log.txt .
dos2unix tac_log.txt
dos2unix rta_log.txt
dos2unix modules.txt
dos2unix classes.txt
dos2unix methods.txt

cd ..
tar czf $data_dir.tgz $data_dir 
