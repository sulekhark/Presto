#!/bin/bash
# Script is executing inside FaultInjectionSet

faultInjectInfoFile=$1
faultAllocInfoFile=$2
tempFileName="temp_out"
dirFileName="fault_config_dirs.txt"
methFileName="id_to_method_map.txt"
excFileName="id_to_exctype_map.txt"

../../../DynamicConfig/scripts/parse_finject.py $faultInjectInfoFile $tempFileName $dirFileName $methFileName $excFileName 
while read -r line
do
    dirName="$(echo $line | cut -d'@' -f1)"    
    caller="$(echo $line | cut -d'@' -f2)"    
    callee="$(echo $line | cut -d'@' -f4)"    
    excType="$(echo $line | cut -d'@' -f5)"    

    mkdir $dirName
    cp -r ../../../DynamicConfig/FaultInjectionTemplate/* $dirName 
    ../../../DynamicConfig/scripts/logging_config.sh $dirName ../assemblies.txt
    sed "s/DAFFODIL_TORCH_CALLEE/$callee/g" $dirName/torch-instrumentation.torchconfig > t
    mv t $dirName/torch-instrumentation.torchconfig
    sed "s/DAFFODIL_TORCH_CALLEE/$callee/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
    sed "s/DAFFODIL_TORCH_CALLER/$caller/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
    allocInfoLine=`grep "$excType" $faultAllocInfoFile`
    allocInfo="$(echo $allocInfoLine | cut -d':' -f2)"    
    sed "s/DAFFODIL_TORCH_EXCEPTION_EXPRESSION/$allocInfo/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
done < $tempFileName
