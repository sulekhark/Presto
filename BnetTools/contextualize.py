#!/usr/bin/env python3

# ./contextualize.py < named_cons_cr_lf_cr.txt.ee > named_cons_cr_lf_cr_ctxt.txt.ee  

import logging
import re
import sys

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

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

########################################################################################################################
# 1. Accept input

allClauses = set()
allRuleNames = {}
allTuples = set()
allConsequents = set()

for line in sys.stdin:
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

logging.info('Loaded {0} clauses.'.format(len(allClauses)))
logging.info('Discovered {0} tuples.'.format(len(allTuples)))
logging.info('Discovered {0} consequents.'.format(len(allConsequents)))
logging.info('Discovered {0} input tuples.'.format(len(allInputTuples)))

########################################################################################################################

def getCATuple(clause):
    for lit in clause:
        tup = lit2Tuple(lit)
        if (tup.startswith('CallAt')):
            return tup
        else:
            return None


def getArgs(tup, num):
    argsStr = tup.split('(')[1]
    argsStr = argsStr[:-1] # remove trailing ')'
    args = argsStr.split(',')
    if num == 0 or num >= len(args):
        return args
    else:
        return args[:num]

 
def generateContextualizedTuple(calleeTuple, ctxtTuple):
    calleeArgs = getArgs(calleeTuple, 0) # get all arguments
    ctxtArgs = getArgs(ctxtTuple, 2) # get the first two arguments
    newArgs = ctxtArgs + calleeArgs
    newTuple = "CtxtCallAt(" + ','.join(newArgs) + ')'
    return newTuple


def replaceWithCtxt(clauseList, ctxtTuple):
    ndx = 0
    for lit in clauseList:
        if "CallAt" in lit:
            break;
        else:
            ndx = ndx + 1
    if "NOT" in clauseList[ndx]:
        clauseList[ndx] = "NOT " + ctxtTuple
    else:
        clauseList[ndx] = ctxtTuple 

########################################################################################################################

consumingClauses = {}
for clause in allClauses:
    consequent = clause2Consequent(clause)
    consumers = []
    for cons in allClauses:
        antecedents = clause2Antecedents(cons)
        for ant in antecedents:
            if consequent in ant:
                consumers.append(cons)
    if len(consumers) == 0:
        consumers.append(clause)
    consumingClauses[clause] = consumers

for clause, consumers in consumingClauses.items():
    ruleName = allRuleNames[clause]
    calleeAtTuple = getCATuple(clause)
    for cons in consumers:
        contextTuple = getCATuple(cons)
        ctxtTuple = generateContextualizedTuple(calleeAtTuple, contextTuple)
        clauseList = list(clause)
        replaceWithCtxt(clauseList, ctxtTuple)
        print("{0}: {1}".format(ruleName, ', '.join(clauseList)))
