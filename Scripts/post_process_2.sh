#!/bin/bash

. $PRESTO_HOME/Configurations/presto_cfg.sh
dynlogTgz=$1
tar xzf $dynlogTgz
cd bnet
dynLogDir=../dynlogs

if [ -s named_cons_cr_lf_cr.txt.ee.refined ]
then
    $PRESTO_HOME/BnetTools/cons_all2bnet.py bnet_dict.out.refined narrowor < named_cons_cr_lf_cr.txt.ee.refined > named_bnet.out.refined
    grep "CallAt" prob_edb_tuples.txt.refined > set1.txt.refined
    grep -v "CallAt" prob_edb_tuples.txt.refined > set2.txt.refined
    rm -f edb_probabilities.txt.refined
    if [ -s set1.txt ]
    then
        $PRESTO_HOME/DynamicConfig/scripts/find_tuple_prob.py set1.txt.refined bnet_dict.out.refined > set1_probs.txt.refined
        cat set1_probs.txt.refined >> edb_probabilities.txt.refined
    fi
    if [ -s set2.txt ]
    then
        $PRESTO_HOME/DynamicConfig/scripts/find_tuple_prob.py set2.txt.refined bnet_dict.out.refined > set2_probs.txt.refined
        cat set2_probs.txt.refined >> edb_probabilities.txt.refined
    fi
    $PRESTO_HOME/BnetTools/bnet2fg.py edb_probabilities.txt.refined  < named_bnet.out.refined > factorGraph.fg.refined 2> bnet2fg.log.refined
    $PRESTO_HOME/BnetTools/arcmd $PRESTO_HOME/BnetTools bnet_dict.out.refined factorGraph.fg.refined base_queries.txt 500
    mv alarm_ranking.txt alarm_ranking.txt.refined
    mv arcmd.log arcmd.log.refined
    mv arcmd.out arcmd.out.refined
fi


$PRESTO_HOME/BnetTools/cons_all2bnet.py bnet_dict.out narrowor < named_cons_cr_lf_cr.txt.ee > named_bnet.out
grep "CallAt" prob_edb_tuples.txt > set1.txt
grep -v "CallAt" prob_edb_tuples.txt > set2.txt
rm -f edb_probabilities.txt
if [ -s set1.txt ]
then
    $PRESTO_HOME/DynamicConfig/scripts/find_tuple_prob.py set1.txt bnet_dict.out > set1_probs.txt
    cat set1_probs.txt >> edb_probabilities.txt
fi
if [ -s set2.txt ]
then
    $PRESTO_HOME/DynamicConfig/scripts/find_tuple_prob.py set2.txt bnet_dict.out > set2_probs.txt
    cat set2_probs.txt >> edb_probabilities.txt
fi
$PRESTO_HOME/BnetTools/bnet2fg.py edb_probabilities.txt  < named_bnet.out > factorGraph.fg 2> bnet2fg.log
$PRESTO_HOME/BnetTools/arcmd $PRESTO_HOME/BnetTools bnet_dict.out factorGraph.fg base_queries.txt 500
