#!/bin/bash

dynlogTgz=$1
tar xzf $dynlogTgz

cd bnet
../../BnetTools/cons_all2bnet.py bnet_dict.out narrowor < named_cons_cr_lf_cr.txt.ee > named_bnet.out
../../BnetTools/cons_all2bnet.py bnet_dict_ctxt.out narrowor < named_cons_cr_lf_cr.ctxt.ee > named_bnet_ctxt.out
../../DynamicConfig/scripts/find_tuple_prob.sh prob_edb_tuples.txt bnet_dict.out edb_prob.txt
../../DynamicConfig/scripts/find_tuple_prob.sh prob_ctxt_edb_tuples.txt bnet_dict_ctxt.out ctxt_edb_prob.txt

../../BnetTools/bnet2fg.py edb_prob.txt  < named_bnet.out > factorGraph.fg 2> bnet2fg.log
../../BnetTools/bnet2fg.py ctxt_edb_prob.txt  < named_bnet_ctxt.out > factorGraph_ctxt.fg 2> bnet2fg_ctxt.log

../../BnetTools/arcmd ../../BnetTools bnet_dict_ctxt.out factorGraph_ctxt.fg base_queries.txt 500
mv alarm_ranking.txt alarm_ranking_ctxt.txt
mv arcmd.log arcmd_ctxt.log
mv arcmd.out arcmd_ctxt.out
../../BnetTools/arcmd ../../BnetTools bnet_dict.out factorGraph.fg base_queries.txt 500
