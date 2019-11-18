#!/bin/bash

dlogTgz=$1
tar xzf $dlogTgz
rm -rf bnet
mkdir bnet
cp datalog/ext_edb.txt bnet

cd datalog
. ./run_all.sh
. ./run_dbg_flows.sh
python3 ./extract_dgraph.py ExcAnalysis_inst.datalog ../bnet

cd ../bnet
#../../BnetTools/get_coreachable/get_coreachable named_cons_all.txt named_cons_coreach.txt base_queries.txt 0 0
cp named_cons_all.txt named_cons_coreach.txt
../../BnetTools/dfs_cycle_elim/dfs_cycle_elim named_cons_coreach.txt named_cons_loopfree.txt rule_prob.txt rule_prob_out.txt
#../../BnetTools/get_coreachable/get_coreachable named_cons_loopfree.txt named_cons_cr_lf_cr.txt base_queries.txt 0 0
cp named_cons_loopfree.txt named_cons_cr_lf_cr.txt
../../BnetTools/elide_edb_ext.py ext_edb.txt < named_cons_cr_lf_cr.txt > named_cons_cr_lf_cr.txt.ee
../../BnetTools/get_edb_tuples.py prob_edb_tuples.txt < named_cons_cr_lf_cr.txt.ee
grep "R1:" named_cons_cr_lf_cr.txt.ee > named_cons_cr_lf_cr.txt.ee.R1
../../BnetTools/contextualize.py < named_cons_cr_lf_cr.txt.ee.R1 > named_cons_cr_lf_cr.ctxt.ee 
grep -v "R1:" named_cons_cr_lf_cr.txt.ee >> named_cons_cr_lf_cr.ctxt.ee
../../BnetTools/get_edb_tuples.py prob_ctxt_edb_tuples.txt < named_cons_cr_lf_cr.ctxt.ee

cd ..
rm -rf dynconfig
mkdir dynconfig
cd dynconfig
../../DynamicConfig/scripts/get_assemblies.sh ../datalog/modules.txt assemblies.txt

mkdir Logging
cp -r ../../DynamicConfig/LoggingTemplate/* Logging
../../DynamicConfig/scripts/logging_config.sh Logging assemblies.txt

mkdir FaultInjectionSet
cd FaultInjectionSet
../../../DynamicConfig/scripts/fault_injection_config.sh ../../datalog/FInject.txt ../../datalog/ExceptionInjection.txt

cd ../..
tar czf dynconfig.tgz dynconfig
