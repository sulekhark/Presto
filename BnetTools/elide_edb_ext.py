#!/usr/bin/env python3

# Given a set of constraints as input on stdin, computes which tuples are EDBs, and elides them while printing to
# stdout. This rewrite already implicitly happens within bnet2fg.py, and it is helpful to have it explicitly as a
# script.

# ./scripts/bnet/summarization/elide_edb_ext.py [prob_edb_file] < named_cons_all.txt > named_cons_all.txt.elided

import logging
import re
import sys
import os


if len(sys.argv) > 1:
    probEdbFileName = sys.argv[1]
else:
    probEdbFileName = ""

removeEdbTransitive = False
clampRule = os.environ['CLAMP_RULE_PROB_TO_1']
if clampRule == "true":
    removeEdbTransitive = True

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

if not probEdbFileName == "":
    probEdbTuples = { line.strip() for line in open(probEdbFileName) if len(line.strip()) > 0 }
else:
    probEdbTuples = set()

def lit2Tuple(literal):
    return literal if not literal.startswith('NOT ') else literal[len('NOT '):]

def clause2Antecedents(clause):
    return [ lit2Tuple(literal) for literal in clause[:-1] ]

def clause2Consequent(clause):
    consequent = clause[-1]
    assert not consequent.startswith('NOT ')
    return consequent

def isProbEdb(t):
   for probEdbTup in probEdbTuples:
      if t.startswith(probEdbTup):
         return True
   return False

def isIdb(t):
   if t in allConsequents:
      return True
   return False

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
    return tuple([ lit for lit in clause if (isIdb(lit2Tuple(lit)) or isProbEdb(lit2Tuple(lit))) ])

change = True
currSimplifiedClauses = allClauses
currSimplifiedRuleNames = allRuleNames

while change:
    change = False
    newSimplifiedClauses = set()
    newSimplifiedRuleNames = {}
    for clause in currSimplifiedClauses:
        sc = simplifyClause(clause, allConsequents)
        postLen = len(sc)
        # If, after simplification, the clause len == 1, and removeEdbTransitive is on,
        # then the clause itself can be deleted.
        # Only if a clause is deleted, a subsequent pass is required.
        if (postLen == 1) and removeEdbTransitive:
            change = True
            # remove sc - i.e. don't add it to newSimplifiedClauses.
        else:
            newSimplifiedClauses.add(sc)
            newSimplifiedRuleNames[sc] = currSimplifiedRuleNames[clause]
    if change:
        allConsequents = { clause2Consequent(clause) for clause in newSimplifiedClauses }
        currSimplifiedClauses = newSimplifiedClauses
        currSimplifiedRuleNames = newSimplifiedRuleNames

########################################################################################################################
# 3. Print output

for clause in newSimplifiedClauses:
    ruleName = newSimplifiedRuleNames[clause]
    clauseStr = ', '.join(clause)
    print('{0}: {1}'.format(ruleName, clauseStr))
