#!/bin/bash

inval=$1
tmp=`bc <<< "scale=2; ($inval * 1.0 / 1024)"`
kval=`bc <<< "scale=1; (($tmp + 0.05)/1)"`
echo $kval
