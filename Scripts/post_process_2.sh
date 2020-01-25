#!/bin/bash

. $PRESTO_HOME/Configurations/presto_cfg.sh
dynlogTgz=$1
tar xzf $dynlogTgz
cd bnet
dynLogDir=../dynlogs

if [ -s named_cons_cr_lf_cr.txt.ee.refined ]
then
    $PRESTO_HOME/BnetTools/cons_all2bnet.py bnet_dict.out.refined narrowor < named_cons_cr_lf_cr.txt.ee.refined > named_bnet.out.refined
    grep -v "^Cond" prob_edb_tuples.txt.refined > set1.txt
    grep  "^CondCallAt" prob_edb_tuples.txt.refined > set2.txt
    grep  "^CondEscape" prob_edb_tuples.txt.refined > set3.txt
    $PRESTO_HOME/DynamicConfig/scripts/find_tuple_prob.sh set1.txt bnet_dict.out.refined edb_probabilities.txt.refined
    if [ -s set2.txt ]
    then
        find $dynLogDir/Logging -name torch\* > set2_logfiles.txt
        $PRESTO_HOME/DynamicConfig/scripts/find_conditional_prob_1.py set2_logfiles.txt set2.txt bnet_dict.out.refined > set2_probs.txt.refined
        cat set2_probs.txt.refined >> edb_probabilities.txt.refined
    fi
    if [ -s set3.txt ]
    then
        $PRESTO_HOME/DynamicConfig/scripts/find_conditional_prob_2.py set3.txt bnet_dict.out.refined > set3_probs.txt.refined
        set3_probs.txt.refined >> edb_probabilities.txt.refined
    fi
    $PRESTO_HOME/BnetTools/bnet2fg.py edb_probabilities.txt.refined  < named_bnet.out.refined > factorGraph.fg.refined 2> bnet2fg.log.refined
    $PRESTO_HOME/BnetTools/arcmd $PRESTO_HOME/BnetTools bnet_dict.out.refined factorGraph.fg.refined base_queries.txt 500
    mv alarm_ranking.txt alarm_ranking.txt.refined
    mv arcmd.log arcmd.log.refined
    mv arcmd.out arcmd.out.refined
fi


$PRESTO_HOME/BnetTools/cons_all2bnet.py bnet_dict.out narrowor < named_cons_cr_lf_cr.txt.ee > named_bnet.out
$PRESTO_HOME/DynamicConfig/scripts/find_tuple_prob.sh prob_edb_tuples.txt bnet_dict.out edb_probabilities.txt
$PRESTO_HOME/BnetTools/bnet2fg.py edb_probabilities.txt  < named_bnet.out > factorGraph.fg 2> bnet2fg.log
$PRESTO_HOME/BnetTools/arcmd $PRESTO_HOME/BnetTools bnet_dict.out factorGraph.fg base_queries.txt 500
