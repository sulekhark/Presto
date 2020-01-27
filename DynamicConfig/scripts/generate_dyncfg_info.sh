#!/bin/bash

# Executes in the "datalog" directory of the benchmark.

appProbRefinedEdbTuplesFile=$1

sed 's/\(.\)$/\1\./' $appProbRefinedEdbTuplesFile > app_prob_refined_edb_tuples.datalog
z3 dyncfg_info_tuples.datalog > dyncfg_info_tuples.out
python3 parse_z3_out.py dyncfg_info_tuples.datalog dyncfg_info_tuples.out >> full_run.out 2>&1
