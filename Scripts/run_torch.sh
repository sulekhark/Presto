#!/bin/bash

# Will execute in benchmark directory
DynCfgTgz=$1
cmdsFile="../../$2" # Will execute in bin/Debug

tar xzf $DynCfgTgz 
rm -rf dynlogs
mkdir dynlogs
cd dynlogs
mkdir Logging
ldir1="../../dynlogs/Logging"
mkdir FaultInjectionSet
mkdir FaultInjectionSet/FInject
mkdir FaultInjectionSet/LinkInject
cd ..

cp -r bin/Debug bin/Debug_orig

cd bin/Debug
$TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/Logging/torch-instrumentation.torchconfig  --rc ../../dynconfig/Logging/RuntimeConfig
while read -r cmd
do
    cmdName=`echo $cmd | cut -d':' -f1`
    execCmd=`echo $cmd | cut -d':' -f2`
    eval $execCmd > execution_log 2>&1
    $TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
    mkdir $ldir1/$cmdName
    mv execution_log $ldir1/$cmdName
    mv torch-runtime-log4net.torchlog.decompressed $ldir1/$cmdName
done < $cmdsFile

while read -r dirName
do
    cd ..
    rm -rf Debug
    mkdir Debug
    cp -r Debug_orig/* Debug
    cd Debug

    $TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/FaultInjectionSet/FInject/$dirName/torch-instrumentation.torchconfig  --rc ../../dynconfig/FaultInjectionSet/FInject/$dirName/RuntimeConfig
    ldir2="../../dynlogs/FaultInjectionSet/FInject/$dirName"
    mkdir $ldir2
    while read -r cmd
    do
        cmdName=`echo $cmd | cut -d':' -f1`
        execCmd=`echo $cmd | cut -d':' -f2`
        eval $execCmd > execution_log 2>&1
        $TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
        mkdir $ldir2/$cmdName
        mv execution_log $ldir2/$cmdName
        mv torch-runtime-log4net.torchlog.decompressed $ldir2/$cmdName
    done < $cmdsFile
done < ../../dynconfig/FaultInjectionSet/FInject/fault_config_dirs.txt

while read -r dirName
do
    cd ..
    rm -rf Debug
    mkdir Debug
    cp -r Debug_orig/* Debug
    cd Debug

    $TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/FaultInjectionSet/LinkInject/$dirName/torch-instrumentation.torchconfig  --rc ../../dynconfig/FaultInjectionSet/LinkInject/$dirName/RuntimeConfig
    ldir3="../../dynlogs/FaultInjectionSet/LinkInject/$dirName"
    mkdir $ldir3
    while read -r cmd
    do
        cmdName=`echo $cmd | cut -d':' -f1`
        execCmd=`echo $cmd | cut -d':' -f2`
        eval $execCmd > execution_log 2>&1
        $TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
        mkdir $ldir3/$cmdName
        mv execution_log $ldir3/$cmdName
        mv torch-runtime-log4net.torchlog.decompressed $ldir3/$cmdName
    done < $cmdsFile
done < ../../dynconfig/FaultInjectionSet/LinkInject/fault_config_dirs.txt

# Restore original state - i.e. before instrumentation
cd ..
rm -rf Debug
mv Debug_orig Debug

cd ..
tar czf dynlogs.tgz dynlogs
