#!/bin/bash

dlogTgz=$1
tar xzf $dlogTgz
rm -rf bnet
mkdir bnet
cp datalog/prob_edb.txt bnet

cd datalog
. ./run_all.sh
. ./run_dbg_flows.sh
python3 ./extract_dgraph.py ExcAnalysis_inst.datalog ../bnet

cd ../bnet
#$DAFFODIL_HOME/BnetTools/get_coreachable/get_coreachable named_cons_all.txt named_cons_coreach.txt base_queries.txt 0 0
cp named_cons_all.txt named_cons_coreach.txt
$DAFFODIL_HOME/BnetTools/dfs_cycle_elim/dfs_cycle_elim named_cons_coreach.txt named_cons_loopfree.txt rule_prob.txt rule_prob_out.txt
#$DAFFODIL_HOME/BnetTools/get_coreachable/get_coreachable named_cons_loopfree.txt named_cons_cr_lf_cr.txt base_queries.txt 0 0
cp named_cons_loopfree.txt named_cons_cr_lf_cr.txt
$DAFFODIL_HOME/BnetTools/elide_edb_ext.py prob_edb.txt < named_cons_cr_lf_cr.txt > named_cons_cr_lf_cr.txt.ee
$DAFFODIL_HOME/BnetTools/get_edb_tuples.py prob_edb_tuples.txt < named_cons_cr_lf_cr.txt.ee

cd ..
rm -rf dynconfig
mkdir dynconfig
cd dynconfig
$DAFFODIL_HOME/DynamicConfig/scripts/get_assemblies.sh ../datalog/modules.txt assemblies.txt
$DAFFODIL_HOME/DynamicConfig/scripts/generate_maps.sh

mkdir Logging
cp -r $DAFFODIL_HOME/DynamicConfig/LoggingTemplate/* Logging
$DAFFODIL_HOME/DynamicConfig/scripts/logging_config.sh Logging assemblies.txt

mkdir FaultInjectionSet
cd FaultInjectionSet
$DAFFODIL_HOME/DynamicConfig/scripts/fault_injection_config.sh ../../../datalog/FInject.txt ../../../datalog/LinkInject.txt ../../../datalog/ExceptionInjection.txt

cd ../..
tar czf dynconfig.tgz dynconfig
