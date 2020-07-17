#!/bin/bash

rm -f t
for i in `cat bnet/named_cons_all.txt`
do
  echo $i >> t
done
node_cnt=`cat t | grep -v ':' | grep -v NOT | sort | uniq | wc -l`
rm -f t
echo $node_cnt
