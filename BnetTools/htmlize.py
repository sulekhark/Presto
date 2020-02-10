#!/usr/bin/env python3

# ./htmlize.py metadata_dir root_idb.txt display_desc.txt < constraints.txt 

import logging
import re
import sys
import os

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

metadataDir = sys.argv[1]
rootIdbFileName = sys.argv[2]
displayDescFileName = sys.argv[3]
 
refineInfo = os.environ['REFINE_INFO']

inputDefnFileName = metadataDir + "/input_defn.datalog"
domMapExtn = ".out"

htmlOtherTemplDir = os.environ['PRESTO_HOME'] + "/HtmlTemplates/Other"
idbAntHtml = htmlOtherTemplDir + "/idb_antecedent.html"
edbAntHtml = htmlOtherTemplDir + "/edb_antecedent.html"
derivSingleHtml = htmlOtherTemplDir + "/deriv_single.html"
derivTupleHtml = htmlOtherTemplDir + "/deriv_tuple.html"

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
# 1. Accept input. Find all "root" IDB tuples whose derivations should be displayed. Find the set of all relation names.

allClauses = set()
allRuleNames = {}
allTuples = set()
allConsequents = set()
allRelNames = set()
rootIdbRelNames = set()
rootIdbs = {}

for line in open(rootIdbFileName):
    line = line.strip()
    rootIdbRelNames.add(line)
    rootIdbs[line] = set() 

for line in sys.stdin:
    line = line.strip()
    clause = [ literal.strip() for literal in re.split(':|, ', line) ]
    ruleName = clause[0]
    clause = clause[1:]
    clause = tuple(clause)
    allClauses.add(clause)
    allRuleNames[clause] = ruleName

    for literal in clause:
        tup = lit2Tuple(literal)
        allTuples.add(tup)
        tupRelName = tup.split('(')[0]
        allRelNames.add(tupRelName)
        if tupRelName in rootIdbRelNames:
            rootIdbs[tupRelName].add(tup)

    allConsequents.add(clause2Consequent(clause))

allInputTuples = allTuples - allConsequents

logging.info('Loaded {0} clauses.'.format(len(allClauses)))
logging.info('Discovered {0} tuples.'.format(len(allTuples)))
logging.info('Discovered {0} consequents.'.format(len(allConsequents)))
logging.info('Discovered {0} input tuples.'.format(len(allInputTuples)))
logging.info('Discovered {0} relations.'.format(len(allRelNames)))

########################################################################################################################
# 2. Create map: consequent to all producing clauses (only their antecedents) 

producingClauses = {}

for clause in allClauses:
    consequent = clause2Consequent(clause)
    if consequent not in producingClauses:
        producers = []
        producingClauses[consequent] = producers
    else:
        producers = producingClauses[consequent]
    producers.append(clause2Antecedents(clause))


########################################################################################################################
# 3. Record the signatures of all (required) relation names. Create maps for the required DOM entries. 

relSignature = {}
domSet = set()
domMaps = {}

for line in open(inputDefnFileName):
    line = line.strip()
    relName = line.split('(')[0]
    if relName in allRelNames:
       args = getArgs(line)
       domNames = [ arg.split(':')[1] for arg in args ]
       for dn in domNames:
           domSet.add(dn)
       relSignature[relName] = domNames

refineFiles = refineInfo.split(' ')
for refineFileName in refineFiles:
    for line in open(refineFileName):
        line = line.strip()
        if (line.startswith('CONDITIONAL_TUPLE:')):
            tupName = line.split(':')[1]
            relName = tupName.split('(')[0]
            if relName in allRelNames:
               args = getArgs(line)
               domNames = [ arg.split(':')[1] for arg in args ]
               for dn in domNames:
                   domSet.add(dn)
               relSignature[relName] = domNames

for dn in domSet:
    dnDict = {}
    domMaps[dn] = dnDict 
    domMapFileName = metadataDir + "/" + dn + domMapExtn
    for line in open(domMapFileName):
        line = line.strip()
        parts = line.split(':',1)
        dnDict[parts[0]] = parts[1]


########################################################################################################################
# 4. Helper functions to generate the html pages

def getHtlmlFileName(tup):
    tup = tup.replace('(', '_')
    tup = tup.replace(',', '_')
    tup = tup.replace(')', '')
    return tup
    

########################################################################################################################
# 5. Create the html pages.

for consequent, producers in producingClauses.items():
    htmlFileName = getHtmlFileName(consequent)
    conDesc = getTupleDesc(consequent)
    for producer in producers:





