#!/bin/bash
# Executes in the benchmark directory

queryIdbFN="$PRESTO_HOME/Datalog/query_idb.txt"
rootIdbFN="$PRESTO_HOME/Datalog/root_idb.txt"
displayFN="$PRESTO_HOME/Datalog/display_desc.txt"

rm -rf reports
mkdir reports
cd reports
mkdir metadata
cd metadata


# Collect all relation signatures.
for i in `cat $PRESTO_HOME/Datalog/static_analyses.txt`
do
    fn="$PRESTO_HOME/Datalog/$i"
    grep " input" $fn | grep ':' | grep '(' >> rels.txt
    grep " printtuples" $fn | grep ':' | grep '(' >> rels.txt
done
sed 's/ input.*//g' rels.txt | sed 's/ printtuples.*//g' >> rels1.txt
sort rels1.txt | uniq > input_defn.datalog
rm -f rels.txt rels1.txt


# Collect all the constraints that have to be htmlized.
cat ../../bnet/named_cons_cr_lf_cr.txt.ee > constraints.txt
cat ../../bnet/named_cons_cr_lf_cr.txt.ee.refined > constraints.txt.refined
diff --new-line-format="" --unchanged-line-format="" <(sort $rootIdbFN) <(sort $queryIdbFN) > new_idb.txt
if [ -s new_idb.txt ]
then
    for i in `cat new_idb.txt`
    do
        cat ../../datalog/$i.datalog > t
        sed 's/)./)/g' t >> new_queries.txt 
    done
    rm -f t

    $PRESTO_HOME/BnetTools/get_coreachable/get_coreachable ../../bnet/named_cons_all.txt named_cons_cr.txt new_queries.txt 0 0
    touch temp1
    $PRESTO_HOME/BnetTools/dfs_cycle_elim/dfs_cycle_elim named_cons_cr.txt named_cons_cr_lf.txt temp1 temp2 
    $PRESTO_HOME/BnetTools/get_coreachable/get_coreachable named_cons_cr_lf.txt named_cons_cr_lf_cr.txt new_queries.txt 0 0
    $PRESTO_HOME/BnetTools/elide_edb_ext.py ../../bnet/prob_edb.txt < named_cons_cr_lf_cr.txt > named_cons_cr_lf_cr.txt.ee
    rm -f temp1 temp2
    cat named_cons_cr_lf_cr.txt.ee >> constraints.txt
    cat named_cons_cr_lf_cr.txt.ee >> constraints.txt.refined
fi


# Preprocess Dom maps for source mappings
$PRESTO_HOME/Datalog/preprocess_doms.py


# Now invoke htmlize.py
cd ..
$PRESTO_HOME/BnetTools/htmlize.py metadata $rootIdbFN $displayFN < metadata/constraints.txt

# Constraints may be shared between refined and non-refined but the probabilities will be different. Hence, different dir.
mkdir refined
cd refined
$PRESTO_HOME/BnetTools/htmlize.py ../metadata $rootIdbFN $displayFN < ../metadata/constraints.txt.refined
cd ../..
