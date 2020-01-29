#!/bin/bash

# Will execute in benchmark directory
DynCfgTgz=$1
cmdsFile=$2

tar xzf $DynCfgTgz 
rm -rf dynlogs
mkdir dynlogs
cd dynlogs
mkdir Logging
ldir1="../../dynlogs/Logging"
mkdir FaultInjectionSet
mkdir FaultInjectionSet/EscapeMTP
mkdir FaultInjectionSet/LinkedEx
cd ..

cp -r bin/Debug bin/Debug_orig

cd bin/Debug
$TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/Logging/torch-instrumentation.torchconfig  --rc ../../dynconfig/Logging/RuntimeConfig
for cmd in `cat $cmdsFile`
do
    cmdName="$(echo $cmd | cut -d':' -f1)"
    execCmd="$(echo $cmd | cut -d':' -f2)"
    eval $execCnd > execution_log 2>&1
    $TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
    mkdir $ldir1/$cmdName
    mv execution_log $ldir1/$cmdName
    mv torch-runtime-log4net.torchlog.decompressed $ldir1/$cmdName
done

while read -r dirName
do
    cd ..
    rm -rf Debug
    mkdir Debug
    cp Debug_orig/* Debug
    cd Debug

    $TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/FaultInjectionSet/EscapeMTP/$dirName/torch-instrumentation.torchconfig  --rc ../../dynconfig/FaultInjectionSet/EscapeMTP/$dirName/RuntimeConfig
    ldir2="../../dynlogs/FaultInjectionSet/FInject/$dirName"
    mkdir $ldir2
    for cmd in `cat $cmdsFile`
    do
        cmdName="$(echo $cmd | cut -d':' -f1)"
        execCmd="$(echo $cmd | cut -d':' -f2)"
        eval $execCnd > execution_log 2>&1
        $TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
        mkdir $ldir2/$cmdName
        mv execution_log $ldir2/$cmdName
        mv torch-runtime-log4net.torchlog.decompressed $ldir2/$cmdName
    done
done < ../../dynconfig/FaultInjectionSet/FInject/fault_config_dirs.txt

while read -r dirName
do
    cd ..
    rm -rf Debug
    mkdir Debug
    cp Debug_orig/* Debug
    cd Debug

    $TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/FaultInjectionSet/LinkedEx/$dirName/torch-instrumentation.torchconfig  --rc ../../dynconfig/FaultInjectionSet/LinkedEx/$dirName/RuntimeConfig
    ldir3="../../dynlogs/FaultInjectionSet/LinkInject/$dirName"
    mkdir $ldir3
    for cmd in `cat $cmdsFile`
    do
        cmdName="$(echo $cmd | cut -d':' -f1)"
        execCmd="$(echo $cmd | cut -d':' -f2)"
        eval $execCnd > execution_log 2>&1
        $TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
        mkdir $ldir3/$cmdName
        mv execution_log $ldir3/$cmdName
        mv torch-runtime-log4net.torchlog.decompressed $ldir3/$cmdName
    done
done < ../../dynconfig/FaultInjectionSet/LinkedEx/fault_config_dirs.txt

# Restore original state - i.e. before instrumentation
cd ..
rm -rf Debug
mv Debug_orig Debug

cd ..
tar czf dynlogs.tgz dynlogs
