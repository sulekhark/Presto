#!/bin/bash

# Will execute in E2EDemo directory
DynCfgTgz=$1

tar xzf $DynCfgTgz 
rm -rf dynlogs
mkdir dynlogs
cd dynlogs
mkdir Logging
mkdir FaultInjectionSet
mkdir FaultInjectionSet/EscapeMTP
mkdir FaultInjectionSet/LinkedEx
cd ..

cp -r bin/Debug bin/Debug_orig

cd bin/Debug
$TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/Logging/torch-instrumentation.torchconfig  --rc ../../dynconfig/Logging/RuntimeConfig
./E2EDemo.exe > execution_log 2>&1
$TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
mv execution_log ../../dynlogs/Logging
mv torch-runtime-log4net.torchlog.decompressed ../../dynlogs/Logging

while read -r dirName
do
    cd ..
    rm -rf Debug
    mkdir Debug
    cp Debug_orig/* Debug
    cd Debug

    $TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/FaultInjectionSet/EscapeMTP/$dirName/torch-instrumentation.torchconfig  --rc ../../dynconfig/FaultInjectionSet/EscapeMTP/$dirName/RuntimeConfig
    ./E2EDemo.exe > execution_log 2>&1
    $TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
    mkdir ../../dynlogs/FaultInjectionSet/EscapeMTP/$dirName
    mv execution_log ../../dynlogs/FaultInjectionSet/EscapeMTP/$dirName
    mv torch-runtime-log4net.torchlog.decompressed ../../dynlogs/FaultInjectionSet/EscapeMTP/$dirName
done < ../../dynconfig/FaultInjectionSet/EscapeMTP/fault_config_dirs.txt

while read -r dirName
do
    cd ..
    rm -rf Debug
    mkdir Debug
    cp Debug_orig/* Debug
    cd Debug

    $TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/FaultInjectionSet/LinkedEx/$dirName/torch-instrumentation.torchconfig  --rc ../../dynconfig/FaultInjectionSet/LinkedEx/$dirName/RuntimeConfig
    ./E2EDemo.exe > execution_log 2>&1
    $TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
    mkdir ../../dynlogs/FaultInjectionSet/LinkedEx/$dirName
    mv execution_log ../../dynlogs/FaultInjectionSet/LinkedEx/$dirName
    mv torch-runtime-log4net.torchlog.decompressed ../../dynlogs/FaultInjectionSet/LinkedEx/$dirName
done < ../../dynconfig/FaultInjectionSet/LinkedEx/fault_config_dirs.txt

# Restore original state - i.e. before instrumentation
cd ..
rm -rf Debug
mv Debug_orig Debug

cd ..
tar czf dynlogs.tgz dynlogs
