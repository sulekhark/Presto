#!/usr/bin/env python3

# ./refine.py [edb_to_refine_file_name] < named_cons_cr_lf_cr.txt.ee > named_cons_cr_lf_cr_refined.txt.ee  

import logging
import re
import sys
import os

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

refineInfoFileName = os.environ['REFINE_INFO']
newSuffix = '_n'

if len(sys.argv) > 1:
    refineEdbFileName = sys.argv[1]
else:
    refineEdbFileName = ""
 
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


def getArgs(tup):
    argsStr = tup.split('(')[1]
    argsStr = argsStr[:-1] # remove trailing ')'
    args = argsStr.split(',')
    return args

 
########################################################################################################################
# 2. Pre-process configuration settings

def computeConditionalArgs(argsOut, argsIn1, argsIn2):
    condArgInfo = []
    for elem in argsOut:
        if elem in argsIn1:
            entry = tuple([1, argsIn1.index(elem)])
            condArgInfo.append(entry)
        elif elem in argsIn2:
            entry = tuple([2, argsIn2.index(elem)])
            condArgInfo.append(entry)
    return condArgInfo


refineRelStr = ''
refineRuleStrA = ''
refineRuleStrB = ''
with open(refineInfoFileName, 'r') as refineInfoFile:
    for line in refineInfoFile.readlines():
        if '#' in line:
            continue
        elif 'CONDITIONAL_ON' in line:
            refineRelStr = line
        elif 'RULE_A:' in line:
            refineRuleStrA = line
        elif 'RULE_B:' in line:
            refineRuleStrB = line

tuple1RelName = refineRelStr.split(' ')[0].strip()
tuple2RelName = refineRelStr.split(' ')[2].strip()
condTuple1RelName = refineRuleStrA.split(' ',2)[1].split('(')[0].strip()
condConseq1RelName = refineRuleStrB.split(' ',2)[1].split('(')[0].strip()
tupsA = refineRuleStrA.split(' ')
condArgInfoTuple1 = computeConditionalArgs(getArgs(tupsA[1]), getArgs(tupsA[3][:-1]), getArgs(tupsA[4][:-2]))
tupsB = refineRuleStrB.split(' ')
condArgInfoConseq1 = computeConditionalArgs(getArgs(tupsB[1]), getArgs(tupsB[3][:-1]), getArgs(tupsB[4][:-2]))

logging.info('Tuple1RelName: {0}'.format(tuple1RelName))
logging.info('Tuple2RelName: {0}'.format(tuple2RelName))
logging.info('ConditionalTuple1RelName: {0}'.format(condTuple1RelName))
logging.info('ConditionalConseq1RelName: {0}'.format(condConseq1RelName))
logging.info('Conditional Tuple1 Args: {0}'.format(condArgInfoTuple1))
logging.info('Conditional Conseq1 Args: {0}'.format(condArgInfoConseq1))


########################################################################################################################
# 2. Accept input

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

edbsToBeRefined = set()
if refineEdbFileName != "":
    for line in open(refineEdbFileName):
        edbsToBeRefined.add(line.strip())
else:
    for tup in allInputTuples:
        if tup.startswith(tuple1RelName):
            edbsToBeRefined.add(tup)

logging.info('Loaded {0} clauses.'.format(len(allClauses)))
logging.info('Discovered {0} tuples.'.format(len(allTuples)))
logging.info('Discovered {0} consequents.'.format(len(allConsequents)))
logging.info('Discovered {0} input tuples.'.format(len(allInputTuples)))
logging.info('Discovered {0} EDB tuples to be refined.'.format(len(edbsToBeRefined)))

########################################################################################################################
# 3. Compute refined rules

def generateConditionalTuple(condTupleRelName, condTupleArgInfo, tuple1Args, tuple2Args):
    newArgs = []
    for arg, index in condTupleArgInfo:
        if arg == 1:
            newArgs.append(tuple1Args[index])
        elif arg == 2:
            newArgs.append(tuple2Args[index])
    newTuple = condTupleRelName + "(" + ','.join(newArgs) + ')'
    return newTuple


def replaceSingle(clauseList, toReplace, replaceWith):
    newClauseList = []
    for lit in clauseList:
        if toReplace in lit:
            if "NOT" in lit:
                newClauseList.append("NOT " + replaceWith)
            else:
                newClauseList.append(replaceWith)
        else:
            newClauseList.append(lit)
    return newClauseList


def replaceDouble(clauseList, toReplace1, replaceWith1, toReplace2, replaceWith2):
    newClauseList = []
    for lit in clauseList:
        if toReplace1 in lit:
            if "NOT" in lit:
                newClauseList.append("NOT " + replaceWith1)
            else:
                newClauseList.append(replaceWith1)
        elif toReplace2 in lit:
            if "NOT" in lit:
                newClauseList.append("NOT " + replaceWith2)
            else:
                newClauseList.append(replaceWith2)
        else:
            newClauseList.append(lit)
    return newClauseList


consumingClauses = {}
processedConsumers = {}
processedProducers = {}
producerSet = set()
consumerSet = set()
retainSet = set()
newRuleSet = set()

for clause in allClauses:
    tup1 = getTuple(clause, tuple1RelName)
    if tup1 != None:
        # print(clause)
        consequent = clause2Consequent(clause)
        consumers = []
        for cons in allClauses:
            antecedents = clause2Antecedents(cons)
            if (getTuple(antecedents, consequent) != None) and (getTuple(antecedents, tuple2RelName) != None):
                consumers.append(cons)
        # print(consumers)
        if len(consumers) > 0:
            if tup1 in edbsToBeRefined:
                consumingClauses[clause] = consumers
                consumerSet = consumerSet.union(consumers)
                producerSet.add(clause)
            else:
                retainSet = retainSet.union(consumers)

# print('-------------------------------------')

newRuleSet = (allClauses - consumerSet - producerSet) | retainSet


for clause, consumers in consumingClauses.items():
    clauseInConsumerSet = clause in consumerSet
    clauseInProcessedConsumers = clause in processedConsumers
    ruleName = allRuleNames[clause]
    if clauseInProcessedConsumers:
        clause = processedConsumers[clause].pop(0)
    newRuleName = ruleName + newSuffix
    tuple1 = getTuple(clause, tuple1RelName)
    conseq1 = clause2Consequent(clause)
    clauseL = list(clause)
    for conClause in consumers:
        conRuleName = allRuleNames[conClause]
        newConRuleName = conRuleName + newSuffix
        tuple2 = getTuple(conClause, tuple2RelName)
        condTuple1 = generateConditionalTuple(condTuple1RelName, condArgInfoTuple1, getArgs(tuple1), getArgs(tuple2))
        condConseq1 = generateConditionalTuple(condConseq1RelName, condArgInfoConseq1, getArgs(conseq1), getArgs(tuple2))
        newClauseL = replaceDouble(clauseL, tuple1, condTuple1, conseq1, condConseq1)
        newClause = tuple(newClauseL)
        if clauseInConsumerSet and not clauseInProcessedConsumers:
            if clause not in processedProducers:
                processedProducers[clause] = []
            processedProducers[clause].append(newClause)
        else:
            newRuleSet.add(newClause)
            allRuleNames[newClause] = newRuleName

        if (conClause in consumingClauses) and (conClause not in processedProducers): # i.e. if conClause is a potential producer
            conClauseL = list(conClause)
            newConClauseL = replaceSingle(conClauseL, conseq1, condConseq1)
            newConClause = tuple(newConClauseL)
            if conClause not in processedConsumers:
                processedConsumers[conClause] = []
            processedConsumers[conClause].append(newConClause)
            allRuleNames[newConClause] = newConRuleName
        elif conClause in processedProducers:
            for cl in processedProducers[conClause]:
                clL = list(cl)
                newClL = replaceSingle(clL, conseq1, condConseq1)
                newCl = tuple(newClL)
                newRuleSet.add(newCl)
                allRuleNames[newCl] = newRuleName
        else: # conClause is only a consumer
            conClauseL = list(conClause)
            newConClauseL = replaceSingle(conClauseL, conseq1, condConseq1)
            newConClause = tuple(newConClauseL)
            newRuleSet.add(newConClause)
            allRuleNames[newConClause] = newRuleName

for clause in newRuleSet:
    print("{0}: {1}".format(allRuleNames[clause], ', '.join(clause)))
