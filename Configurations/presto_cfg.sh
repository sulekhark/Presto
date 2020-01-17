#!/bin/bash

export CLAMP_RULE_PROB_TO_1=true
export CLAMP_EDB_PROB_TO_1=false
export PROBABILISTIC_RULE_SET=none
export PROBABILISTIC_EDB_SET=$PRESTO_HOME/Datalog/prob_edb.txt
export STATIC_ANALYSIS=$PRESTO_HOME/Datalog/static_analyses.txt
export TRACKED_IDB_SET=$PRESTO_HOME/Datalog/tracked_idb.txt
export QUERY_IDB_SET=$PRESTO_HOME/Datalog/query_idb.txt
export ORACLE=dynamic
export COMPRESS_BNET=false
export REFINE_BNET=true
