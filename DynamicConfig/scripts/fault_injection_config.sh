#!/bin/bash
# Script is executing inside FaultInjectionSet

faultInjectInfoFile=$1
linkInjectInfoFile=$2
faultAllocInfoFile=$3

tempFileName="temp_out_fi"
dirFileName="fault_config_dirs.txt"

mkdir FInject
cd FInject
$PRESTO_HOME/DynamicConfig/scripts/parse_finject.py $faultInjectInfoFile $tempFileName $dirFileName
while read -r line
do
    dirName="$(echo $line | cut -d'@' -f1)"
    callerFull="$(echo $line | cut -d'@' -f2)"
    caller="$(echo $callerFull | cut -d'(' -f1)*"
    invkOffset="$(echo $line | cut -d'@' -f3)"
    calleeFull="$(echo $line | cut -d'@' -f4)"
    callee="$(echo $calleeFull | cut -d'(' -f1)*"
    excType="$(echo $line | cut -d'@' -f5)"

    callerFullM="$(echo "$callerFull" | sed 's/</\\\&lt;/g' | sed 's/>/\\\&gt;/g')"
    calleeFullM="$(echo "$calleeFull" | sed 's/</\\\&lt;/g' | sed 's/>/\\\&gt;/g')"

    mkdir $dirName
    cp -r $PRESTO_HOME/DynamicConfig/FaultInjectionTemplate/* $dirName
    $PRESTO_HOME/DynamicConfig/scripts/logging_config.sh $dirName ../../assemblies.txt
    sed "s/PRESTO_TORCH_CALLEE/$calleeFullM/g" $dirName/torch-instrumentation.torchconfig > t
    mv t $dirName/torch-instrumentation.torchconfig
    sed "s/PRESTO_TORCH_CALLEE/$calleeFullM/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
    sed "s/PRESTO_TORCH_CALLER/$callerFullM/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
    sed "s/PRESTO_INVOKE_OFFSET/$invkOffset/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
    allocInfoLine=`grep "$excType" $faultAllocInfoFile`
    allocInfo="$(echo $allocInfoLine | cut -d':' -f2)"
    allocInfoM="$(echo "$allocInfo" | sed 's/</\\\&lt;/g' | sed 's/>/\\\&gt;/g')"
    sed "s/PRESTO_TORCH_EXCEPTION_EXPRESSION/$allocInfoM/g" $dirName/RuntimeConfig/torch-fault.torchconfig > t
    mv t $dirName/RuntimeConfig/torch-fault.torchconfig
done < $tempFileName
rm -f $tempFileName
cd ..


mkdir LinkInject
cd LinkInject
$PRESTO_HOME/DynamicConfig/scripts/parse_linkinject.py $linkInjectInfoFile $tempFileName $dirFileName
while read -r line
do
    dirName="$(echo $line | cut -d'@' -f1)"
    callerFull="$(echo $line | cut -d'@' -f2)"
    caller="$(echo $callerFull | cut -d'(' -f1)*"
    excType="$(echo $line | cut -d'@' -f3)"
    allocInfoLine=`grep "$excType" $faultAllocInfoFile`
    allocInfo="$(echo $allocInfoLine | cut -d':' -f2)"
    callerFullM="$(echo "$callerFull" | sed 's/</\\\&lt;/g' | sed 's/>/\\\&gt;/g')"
    allocInfoM="$(echo "$allocInfo" | sed 's/</\\\&lt;/g' | sed 's/>/\\\&gt;/g')"

    mkdir $dirName
    cp -r $PRESTO_HOME/DynamicConfig/MultipleFaultsInjectionTemplate/* $dirName 
    mv $dirName/torch-instrumentation.torchconfig_begin $dirName/torch-instrumentation.torchconfig
    $PRESTO_HOME/DynamicConfig/scripts/logging_config.sh $dirName ../../assemblies.txt
    mv $dirName/RuntimeConfig/torch-fault.torchconfig_begin $dirName/RuntimeConfig/torch-fault.torchconfig

    calleeList="$(echo $line | cut -d'@' -f4)"
    IFS='#' read -r -a calleeArray <<< "$calleeList"
    for calleeElem in "${calleeArray[@]}"
    do
        cp $dirName/RuntimeConfig/torch-fault.torchconfig_middle $dirName/RuntimeConfig/calleeFault
        invkOffset="$(echo $calleeElem | cut -d':' -f1)"
        calleeFull="$(echo $calleeElem | cut -d':' -f2)"
        callee="$(echo $calleeFull | cut -d'(' -f1)*"
        calleeFullM="$(echo "$calleeFull" | sed 's/</\\\&lt;/g' | sed 's/>/\\\&gt;/g')"
        sed "s/PRESTO_TORCH_CALLEE/$calleeFullM/g" $dirName/RuntimeConfig/calleeFault > t
        mv t $dirName/RuntimeConfig/calleeFault
        sed "s/PRESTO_TORCH_CALLER/$callerFullM/g" $dirName/RuntimeConfig/calleeFault > t
        mv t $dirName/RuntimeConfig/calleeFault
        sed "s/PRESTO_INVOKE_OFFSET/$invkOffset/g" $dirName/RuntimeConfig/calleeFault > t
        mv t $dirName/RuntimeConfig/calleeFault
        sed "s/PRESTO_TORCH_EXCEPTION_EXPRESSION/$allocInfoM/g" $dirName/RuntimeConfig/calleeFault > t
        mv t $dirName/RuntimeConfig/calleeFault
        cat $dirName/RuntimeConfig/calleeFault >> $dirName/RuntimeConfig/torch-fault.torchconfig
	rm -f $dirName/RuntimeConfig/calleeFault
        sed "s/PRESTO_TORCH_CALLEE/$calleeFullM/g" $dirName/torch-instrumentation.torchconfig_middle >> $dirName/torch-instrumentation.torchconfig
    done
    cat $dirName/RuntimeConfig/torch-fault.torchconfig_end >> $dirName/RuntimeConfig/torch-fault.torchconfig
    cat $dirName/torch-instrumentation.torchconfig_end >> $dirName/torch-instrumentation.torchconfig
    rm -f $dirName/RuntimeConfig/torch-fault.torchconfig_end $dirName/RuntimeConfig/torch-fault.torchconfig_middle
    rm -f $dirName/torch-instrumentation.torchconfig_end $dirName/torch-instrumentation.torchconfig_middle
done < $tempFileName
rm -f $tempFileName
cd ..
