#!/bin/bash

export CLAMP_RULE_PROB_TO_1=true                                     # true | false
export DEFAULT_RULE_PROB=0.99                                        # Note: This applies only if CLAMP_RULE_PROB_TO_1 is set to false.
export PROBABILISTIC_RULE_SET=none                                   # none | <filename> Note: value should be "none" if CLAMP_RULE_PROB_TO_1 is set to true.
export CLAMP_EDB_PROB_TO_1=false                                     # true | false
export DEFAULT_EDB_PROB=0.99                                         # Note: This applies only if CLAMP_EDB_PROB_TO_1 is set to false.
export PROBABILISTIC_EDB_SET=$PRESTO_HOME/Datalog/prob_edb.txt       # none | <filename> Note: value should be "none" if CLAMP_EDB_PROB_TO_1 is set to true.
export STATIC_ANALYSIS=$PRESTO_HOME/Datalog/static_analyses.txt      # <filename> Note: contains the filenames comprising the static analysis.
export TRACKED_IDB_SET=$PRESTO_HOME/Datalog/tracked_idb.txt          # <filename> Note: contains a list of IDB tuple names whose derivations should be modeled by the BNet.
export QUERY_IDB_SET=$PRESTO_HOME/Datalog/query_idb.txt              # <filename> Note: contains a list of IDB tuple names that represent bugs.
export ORACLE=dynamic                                                # dynamic | human
export COMPRESS_BNET=false                                           # true | false
export REFINE_BNET=true                                              # true | false
export CYCLE_ELIM_ALGO=dfs_based                                     # aggressive | dfs_based | precise
