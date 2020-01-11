#!/bin/bash

dynlogTgz=$1
tar xzf $dynlogTgz

cd bnet
$DAFFODIL_HOME/BnetTools/cons_all2bnet.py bnet_dict.out narrowor < named_cons_cr_lf_cr.txt.ee > named_bnet.out
$DAFFODIL_HOME/DynamicConfig/scripts/find_tuple_prob.sh prob_edb_tuples.txt bnet_dict.out edb_prob.txt
$DAFFODIL_HOME/BnetTools/bnet2fg.py edb_prob.txt  < named_bnet.out > factorGraph.fg 2> bnet2fg.log
$DAFFODIL_HOME/BnetTools/arcmd $DAFFODIL_HOME/BnetTools bnet_dict.out factorGraph.fg base_queries.txt 500
