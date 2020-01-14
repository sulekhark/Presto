#!/bin/bash
# Script is executing inside FaultInjectionSet

faultInjectInfoFile=$1
linkInjectInfoFile=$2
faultAllocInfoFile=$3

tempFileName="temp_out_fi"
dirFileName="fault_config_dirs.txt"

mkdir EscapeMTP
cd EscapeMTP
$DAFFODIL_HOME/DynamicConfig/scripts/parse_finject.py $faultInjectInfoFile $tempFileName $dirFileName
while read -r line
do
    dirName="$(echo $line | cut -d'@' -f1)"
    callerFull="$(echo $line | cut -d'@' -f2)"
    caller="$(echo $callerFull | cut -d'(' -f1)*"
    invkOffset="$(echo $line | cut -d'@' -f3)"
    calleeFull="$(echo $line | cut -d'@' -f4)"
    callee="$(echo $calleeFull | cut -d'(' -f1)*"
    excType="$(echo $line | cut -d'@' -f5)"

    mkdir $dirName
    cp -r $DAFFODIL_HOME/DynamicConfig/FaultInjectionTemplate/* $dirName
    $DAFFODIL_HOME/DynamicConfig/scripts/logging_config.sh $dirName ../../assemblies.txt
    sed "s/DAFFODIL_TORCH_CALLEE/$callee/g" $dirName/torch-instrumentation.torchconfig > t
    mv t $dirName/torch-instrumentation.torchconfig
    sed "s/DAFFODIL_TORCH_CALLEE/$callee/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
    sed "s/DAFFODIL_TORCH_CALLER/$caller/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
    sed "s/DAFFODIL_INVOKE_OFFSET/$invkOffset/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
    allocInfoLine=`grep "$excType" $faultAllocInfoFile`
    allocInfo="$(echo $allocInfoLine | cut -d':' -f2)"
    sed "s/DAFFODIL_TORCH_EXCEPTION_EXPRESSION/$allocInfo/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
done < $tempFileName
rm -f $tempFileName
cd ..

mkdir LinkedEx
cd LinkedEx
$DAFFODIL_HOME/DynamicConfig/scripts/parse_linkinject.py $linkInjectInfoFile $tempFileName $dirFileName
while read -r line
do
    dirName="$(echo $line | cut -d'@' -f1)"
    callerFull="$(echo $line | cut -d'@' -f2)"
    caller="$(echo $callerFull | cut -d'(' -f1)*"
    excType="$(echo $line | cut -d'@' -f3)"
    allocInfoLine=`grep "$excType" $faultAllocInfoFile`
    allocInfo="$(echo $allocInfoLine | cut -d':' -f2)"

    mkdir $dirName
    cp -r $DAFFODIL_HOME/DynamicConfig/MultipleFaultsInjectionTemplate/* $dirName 
    mv $dirName/torch-instrumentation.torchconfig_begin $dirName/torch-instrumentation.torchconfig
    $DAFFODIL_HOME/DynamicConfig/scripts/logging_config.sh $dirName ../../assemblies.txt
    mv $dirName/RuntimeConfig/torch-fault.torchconfig_begin $dirName/RuntimeConfig/torch-fault.torchconfig

    calleeList="$(echo $line | cut -d'@' -f4)"
    IFS='#' read -r -a calleeArray <<< "$calleeList"
    for calleeElem in "${calleeArray[@]}"
    do
        cp $dirName/RuntimeConfig/torch-fault.torchconfig_middle $dirName/RuntimeConfig/calleeFault
        invkOffset="$(echo $calleeElem | cut -d':' -f1)"
        calleeFull="$(echo $calleeElem | cut -d':' -f2)"
        callee="$(echo $calleeFull | cut -d'(' -f1)*"
        sed "s/DAFFODIL_TORCH_CALLEE/$callee/g" $dirName/RuntimeConfig/calleeFault > t
        mv t $dirName/RuntimeConfig/calleeFault
        sed "s/DAFFODIL_TORCH_CALLER/$caller/g" $dirName/RuntimeConfig/calleeFault > t
        mv t $dirName/RuntimeConfig/calleeFault
        sed "s/DAFFODIL_INVOKE_OFFSET/$invkOffset/g" $dirName/RuntimeConfig/calleeFault > t
        mv t $dirName/RuntimeConfig/calleeFault
        sed "s/DAFFODIL_TORCH_EXCEPTION_EXPRESSION/$allocInfo/g" $dirName/RuntimeConfig/calleeFault > t
        mv t $dirName/RuntimeConfig/calleeFault
        cat $dirName/RuntimeConfig/calleeFault >> $dirName/RuntimeConfig/torch-fault.torchconfig
	rm -f $dirName/RuntimeConfig/calleeFault
        sed "s/DAFFODIL_TORCH_CALLEE/$callee/g" $dirName/torch-instrumentation.torchconfig_middle >> $dirName/torch-instrumentation.torchconfig 
    done
    cat $dirName/RuntimeConfig/torch-fault.torchconfig_end >> $dirName/RuntimeConfig/torch-fault.torchconfig
    cat $dirName/torch-instrumentation.torchconfig_end >> $dirName/torch-instrumentation.torchconfig
    rm -f $dirName/RuntimeConfig/torch-fault.torchconfig_end $dirName/RuntimeConfig/torch-fault.torchconfig_middle
    rm -f $dirName/torch-instrumentation.torchconfig_end $dirName/torch-instrumentation.torchconfig_middle
done < $tempFileName
rm -f $tempFileName
cd ..
