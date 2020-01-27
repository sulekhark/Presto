#!/bin/bash

. $PRESTO_HOME/Configurations/presto_cfg.sh
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
$PRESTO_HOME/BnetTools/get_coreachable/get_coreachable named_cons_all.txt named_cons_cr.txt base_queries.txt 0 0
$PRESTO_HOME/BnetTools/dfs_cycle_elim/dfs_cycle_elim named_cons_cr.txt named_cons_cr_lf.txt rule_prob.txt rule_prob_out.txt
$PRESTO_HOME/BnetTools/get_coreachable/get_coreachable named_cons_cr_lf.txt named_cons_cr_lf_cr.txt base_queries.txt 0 0
$PRESTO_HOME/BnetTools/elide_edb_ext.py prob_edb.txt < named_cons_cr_lf_cr.txt > named_cons_cr_lf_cr.txt.ee
$PRESTO_HOME/BnetTools/get_edb_tuples.py prob_edb_tuples.txt < named_cons_cr_lf_cr.txt.ee
cd ../datalog
$PRESTO_HOME/DynamicConfig/scripts/find_application_tuples.sh ../bnet/prob_edb_tuples.txt ../bnet/app_prob_edb_tuples.txt
cd ../bnet
$PRESTO_HOME/DynamicConfig/scripts/get_counts.py app_prob_edb_tuples.txt named_cons_cr_lf_cr.txt.ee | sort -nr -k 2 > counts.txt
$PRESTO_HOME/DynamicConfig/scripts/get_edbs_to_refine.py counts.txt app_prob_edbs_to_be_refined.txt app_prob_edbs_no_refine.txt

cp named_cons_cr_lf_cr.txt.ee named_cons_cr_lf_cr.txt.ee.refined
crntInFile=named_cons_cr_lf_cr.txt.ee.refined
tempOutFile="tempOutFile"
for i in $REFINE_INFO
do
    $PRESTO_HOME/BnetTools/refine.py $i rule_dict.txt app_prob_edbs_to_be_refined.txt < $crntInFile > $tempOutFile
    mv $tempOutFile $crntInFile
done
$PRESTO_HOME/BnetTools/get_edb_tuples.py prob_edb_tuples.txt.refined < named_cons_cr_lf_cr.txt.ee.refined

cat app_prob_edbs_no_refine.txt > app_prob_edb_tuples.txt.refined
grep "COND" prob_edb_tuples.txt.refined >> app_prob_edb_tuples.txt.refined
cd ../datalog
$PRESTO_HOME/DynamicConfig/scripts/generate_dyncfg_info.sh ../bnet/app_prob_edb_tuples.txt.refined

cd ..
rm -rf dynconfig
mkdir dynconfig
cd dynconfig
$PRESTO_HOME/DynamicConfig/scripts/get_assemblies.sh ../datalog/modules.txt assemblies.txt
$PRESTO_HOME/DynamicConfig/scripts/generate_maps.sh

mkdir Logging
cp -r $PRESTO_HOME/DynamicConfig/LoggingTemplate/* Logging
$PRESTO_HOME/DynamicConfig/scripts/logging_config.sh Logging assemblies.txt

mkdir FaultInjectionSet
cd FaultInjectionSet
$PRESTO_HOME/DynamicConfig/scripts/fault_injection_config.sh ../../../datalog/FInject.txt ../../../datalog/LinkInject.txt ../../../datalog/ExceptionInjection.txt

cd ../..
tar czf dynconfig.tgz dynconfig
