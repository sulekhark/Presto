#!/bin/bash

# Executes in the "datalog" directory of the benchmark.

probEdbTuplesFile=$1
appProbEdbTuplesFile=$2

$PRESTO_HOME/DynamicConfig/scripts/find_application_methods.py
sed 's/\(.\)$/\1\./' $probEdbTuplesFile > prob_edb_tuples.datalog
z3 dyncfg_app_tuples.datalog > dyncfg_app_tuples.out
python3 parse_z3_out.py dyncfg_app_tuples.datalog dyncfg_app_tuples.out >> full_run.out 2>&1
cat LibEscapeMTP.datalog > lib_prob_edb_tuples.datalog
cat LibCallAt.datalog >> lib_prob_edb_tuples.datalog
cat LibLinkedEx.datalog >> lib_prob_edb_tuples.datalog
sed 's/.$//' lib_prob_edb_tuples.datalog | sed 's/Lib//' > lib_prob_edb_tuples.datalog.txt
# link to below cmd: https://stackoverflow.com/questions/18204904/fast-way-of-finding-lines-in-one-file-that-are-not-in-another
diff --new-line-format="" --unchanged-line-format="" <(sort $probEdbTuplesFile) <(sort lib_prob_edb_tuples.datalog.txt) > $appProbEdbTuplesFile 
