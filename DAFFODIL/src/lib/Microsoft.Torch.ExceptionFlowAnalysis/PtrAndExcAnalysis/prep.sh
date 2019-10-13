#!/bin/bash

data_dir=$1

cd $data_dir
dos2unix CIPtrAnalysis.datalog
echo -n "" >> CIPtrAnalysis.datalog
dos2unix ExcAnalysisInterProc.datalog
echo -n "" >> ExcAnalysisInterProc.datalog
dos2unix ExcAnalysisIntraProc.datalog
echo -n "" >> ExcAnalysisIntraProc.datalog
dos2unix ExcFlows.datalog
echo -n "" >> ExcFlows.datalog
dos2unix DbgFlows.datalog
echo -n "" >> DbgFlows.datalog
dos2unix parse_z3_out.py
echo -n "" >> parse_z3_out.py
dos2unix run_all.sh
echo -n "" >> run_all.sh
dos2unix run_dbg.sh
echo -n "" >> run_dbg.sh
cp ../logs/rta_log.txt .
cp ../logs/tac_log.txt .
dos2unix tac_log.txt

cd ..
tar czf $data_dir.tgz $data_dir 
