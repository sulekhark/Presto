#!/usr/bin/env python3

# Given a set of constraints as input on stdin, computes which tuples are EDBs
# The input set of constraints should have the unnecessary set of tuples elided out.
# We need two sets of EDB tuples:
#   1) All EDB tuples: All of these will be clamped to "true" just before inference.
#   2) The set of EDB tuples for which we will get a probability from the dynamic analysis.

# get_edb_tuples.py all_edb_tuples.txt prob_edb_tuples.txt < named_cons_all.txt.ee

import logging
import re
import sys


allEdbTuplesFileName = sys.argv[1]
probEdbTuplesFileName = sys.argv[2]

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

########################################################################################################################
# 1. Accept input

allClauses = set()
allRuleNames = {}

for line in sys.stdin:
    line = line.strip()
    clause = [ literal.strip() for literal in re.split(':|, ', line) ]
    ruleName, clause = clause[0], tuple(clause[1:])

    allRuleNames[clause] = ruleName
    allClauses.add(clause)

def lit2Tuple(literal):
    return literal if not literal.startswith('NOT ') else literal[len('NOT '):]

def clause2Antecedents(clause):
    return [ lit2Tuple(literal) for literal in clause[:-1] ]

def clause2Consequent(clause):
    consequent = clause[-1]
    assert not consequent.startswith('NOT ')
    return consequent

def isNotExtEdb(t):
   for extEdbTup in extEdbTuples:
      if t.startswith(extEdbTup):
         return False 
   return True 

allTuples = { lit2Tuple(literal) for clause in allClauses for literal in clause }
allConsequents = { clause2Consequent(clause) for clause in allClauses }
allInputTuples = allTuples - allConsequents

logging.info('Loaded {0} clauses.'.format(len(allClauses)))
logging.info('Discovered {0} tuples.'.format(len(allTuples)))
logging.info('Discovered {0} consequents.'.format(len(allConsequents)))
logging.info('Discovered {0} input tuples.'.format(len(allInputTuples)))

########################################################################################################################
# 2. Collect probEdbTuples 

probEdbTuples = set()

for clause in allClauses:
    if allRuleNames[clause] == "R1":
        antecedents = clause2Antecedents(clause)
        for literal in antecedents:
            if not lit2Tuple(literal) in allConsequents:
                probEdbTuples.add(lit2Tuple(literal))

########################################################################################################################
# 3. Print output

with open(allEdbTuplesFileName, 'w') as allEdbTuplesFile:
    for tup in allInputTuples:
        print('{0}'.format(tup), file=allEdbTuplesFile)

with open(probEdbTuplesFileName, 'w') as probEdbTuplesFile:
    for tup in probEdbTuples:
        print('{0}'.format(tup), file=probEdbTuplesFile)
