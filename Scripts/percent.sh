#!/bin/bash

kval=`bc <<< "scale=1; ($1 * 100.0 / $2)"`
echo $kval
