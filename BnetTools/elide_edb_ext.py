#!/usr/bin/env python3

# Given a set of constraints as input on stdin, computes which tuples are EDBs, and elides them while printing to
# stdout. This rewrite already implicitly happens within bnet2fg.py, and it is helpful to have it explicitly as a
# script.

# ./scripts/bnet/summarization/elide_edb_ext.py [ext_edb_file] < named_cons_all.txt > named_cons_all.txt.elided

import logging
import re
import sys


if len(sys.argv) > 1:
    extEdbFileName = sys.argv[1]
else:
    extEdbFileName = ""

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

if not extEdbFileName == "":
    extEdbTuples = { line.strip() for line in open(extEdbFileName) if len(line.strip()) > 0 }
else:
    extEdbTuples = set()

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
# 2. Simplify clauses

def simplifyClause(clause, conseqs):
    return tuple([ lit for lit in clause if (isNotExtEdb(lit2Tuple(lit))) ])

newSimplifiedClauses = set()
newSimplifiedRuleNames = {} 

for clause in allClauses:
    sc = simplifyClause(clause, allConsequents)
    if len(sc) > 1:
        newSimplifiedClauses.add(sc)
        newSimplifiedRuleNames[sc] = allRuleNames[clause]
    else:
        if (isNotExtEdb(clause2Consequent(clause))):
            newSimplifiedClauses.add(sc)
            newSimplifiedRuleNames[sc] = allRuleNames[clause]

########################################################################################################################
# 3. Print output

for clause in newSimplifiedClauses:
    ruleName = newSimplifiedRuleNames[clause]
    clauseStr = ', '.join(clause)
    print('{0}: {1}'.format(ruleName, clauseStr))
