#!/bin/bash

# Will execute in E2EDemo directory
DynCfgTgz=$1

tar xzf $DynCfgTgz 
rm -rf dynlogs
mkdir dynlogs
cd dynlogs
mkdir Logging
mkdir FaultInjectionSet
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

    $TORCH_HOME/tools/net/torch-instrumenter-internal.exe -i . --ic ../../dynconfig/FaultInjectionSet/$dirName/torch-instrumentation.torchconfig  --rc ../../dynconfig/FaultInjectionSet/$dirName/RuntimeConfig
    ./E2EDemo.exe > execution_log 2>&1
    $TORCH_HOME/tools/net/TorchLogAnalyzer.exe decompress -i torch-runtime-log4net.torchlog
    mkdir ../../dynlogs/FaultInjectionSet/$dirName
    mv execution_log ../../dynlogs/FaultInjectionSet/$dirName
    mv torch-runtime-log4net.torchlog.decompressed ../../dynlogs/FaultInjectionSet/$dirName
done < ../../dynconfig/FaultInjectionSet/fault_config_dirs.txt

# Restore original state - i.e. before instrumentation
cd ..
rm -rf Debug
mv Debug_orig Debug

cd ..
tar czf dynlogs.tgz dynlogs
