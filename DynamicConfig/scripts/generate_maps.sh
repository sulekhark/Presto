#!/bin/bash

# Executes in dynconfig

methFileName="id_to_method_map.txt"
excFileName="id_to_exctype_map.txt"

mMapFile="../datalog/M.map"
exMapFile="../datalog/ExceptionType.txt"

sed 's/ExceptionType(//g' $exMapFile > t
sed 's/).//g' t > t1
sed 's/\r//g' t1 > $excFileName
rm -f t t1

rm -f $methFileName
sed 's/  CLASS:.*//g' $mMapFile > t
count=0
while read -r line
do
   echo "$count:$line" >> $methFileName
   count=$((count + 1)) 
done < t
rm -f t
