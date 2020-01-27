#!/usr/bin/env python3

# ./get_counts.py app_prob_edb_tuples.txt named_cons_cr_lf_cr.txt.ee > counts.txt 

import logging
import re
import sys

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

appProbEdbFileName = sys.argv[1]
rulesFileName = sys.argv[2]
 
########################################################################################################################
# 0. Prelude

def lit2Tuple(literal):
    return literal if not literal.startswith('NOT ') else literal[len('NOT '):]

def clause2Antecedents(clause):
    return [ lit2Tuple(literal) for literal in clause[:-1] ]

def clause2Consequent(clause):
    consequent = clause[-1]
    assert not consequent.startswith('NOT ')
    return consequent


def getTuple(clause, tupleRelName):
    for lit in clause:
        tup = lit2Tuple(lit)
        if (tup.startswith(tupleRelName)):
            return tup
    return None


########################################################################################################################
# 1. Read in all the rules 

allClauses = set()
allRuleNames = {}
allTuples = set()
allConsequents = set()

for line in open(rulesFileName):
    line = line.strip()
    clause = [ literal.strip() for literal in re.split(':|, ', line) ]
    ruleName = clause[0]
    clause = clause[1:]
    clause = tuple(clause)
    allClauses.add(clause)
    allRuleNames[clause] = ruleName

    for literal in clause:
        allTuples.add(lit2Tuple(literal))

    allConsequents.add(clause2Consequent(clause))

allInputTuples = allTuples - allConsequents

appProbEdbs = set()
for line in open(appProbEdbFileName):
    line = line.strip()
    appProbEdbs.add(line)

logging.info('Loaded {0} clauses.'.format(len(allClauses)))
logging.info('Discovered {0} tuples.'.format(len(allTuples)))
logging.info('Discovered {0} consequents.'.format(len(allConsequents)))
logging.info('Discovered {0} input tuples.'.format(len(allInputTuples)))
logging.info('Discovered {0} App prob EDB tuples.'.format(len(appProbEdbs)))

########################################################################################################################
# 2. Compute counts 

def tupleSearch(tupList):
    ts = set()
    for tup in tupList:
        if tup in appProbEdbs:
            ts.add(tup)
    return ts


consumingClauses = {}

for clause in allClauses:
    tupSet = tupleSearch(clause2Antecedents(clause))
    for tup in tupSet:
        if tup not in consumingClauses:
            consumingClauses[tup] = set()
        consumingClauses[tup].add(clause)


counts = {}
for tup, clSet in consumingClauses.items():
    consumers = set()
    for clause in clSet:
        consequent = clause2Consequent(clause)
        for cons in allClauses:
            antecedents = clause2Antecedents(cons)
            if (getTuple(antecedents, consequent) is not None):
                consumers.add(cons)
        counts[tup] = len(consumers)


for tup, cnt in counts.items():
    print("{0} {1}".format(tup, cnt))
