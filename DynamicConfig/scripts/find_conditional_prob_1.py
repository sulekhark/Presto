#!/usr/bin/env python3

# We need to parse torch logs and extract the call-tree from the logs.
# We will then compute the probabilities by using the call-tree to count conditional method calls.

# find_conditional_prob.py logsFileName probEdbFileName bnetDictFileName > outFileName
# ASSUMPTION: This script executes in the bnet dir of the benchmark.

import logging
import re
import sys


logsFileName = sys.argv[1]
probEdbFileName = sys.argv[2]
bnetDictFileName = sys.argv[3]

pnMapFileName = "../datalog/PNMap.datalog"
methodMapFileName = "../dynconfig/id_to_method_map.txt"
enclosingCatchFileName = "../datalog/EnclosingEH.datalog"

minProb = 0.5
maxProb = 0.99
midProb = 0.95

logging.basicConfig(level=logging.INFO, \
                            format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                                                datefmt="%H:%M:%S")


########################################################################################################################
# Define the tree data structure for call tree 

from tree import Node 

########################################################################################################################
# Read in the log files 

logFileNameList = [ line.strip() for line in open(logsFileName) ]
logRoots = []
method2NodeMap = {}

def isInstrumented(methName):
    return True

def insertIntoMap(methName, node):
    methName = methName.split('(')[0]   # SRK temp hack until method names are fixed
    if methName not in method2NodeMap:
        method2NodeMap[methName] = []
    method2NodeMap[methName].append(node)

for logFileName in logFileNameList:
    logLines = [line.strip() for line in open(logFileName) ]
    logLines = logLines[1:]
    crntNode = None
    for line in logLines:
        lineParts = line.split(';')
        caller = lineParts[6]
        callee = lineParts[7]
        ilOffset = lineParts[10]
        exception = lineParts[14]
        if (crntNode is None):
            node = Node(data=caller)
            insertIntoMap(caller, node)
            crntNode = node
            logRoots.append(node)
        elif crntNode.data != caller:
            descend = True 
            if isInstrumented(crntNode.data):
                ancestor = crntNode.getAncestor(caller)
                if ancestor is not None:
                    descend = False
                    crntNode = ancestor
            if descend:
                # create the call_tree edge: crntNode.data --> caller
                node = Node(data=caller)
                insertIntoMap(caller, node)
                crntNode.addChild(node, "-1")
                crntNode = node
        calleeNode = Node(data=callee)
        insertIntoMap(callee, calleeNode)
        crntNode.addChild(calleeNode, ilOffset)
        crntNode = calleeNode

# for root in logRoots:
    # root.printTree()


########################################################################################################################
# Read in the various map files 

bnetDictMap = {}
bnetDictEntries = [ line.strip() for line in open(bnetDictFileName) ]
for entry in bnetDictEntries:
    if 'CallAt' in entry:
        parts = entry.split(': ')
        bnetDictMap[parts[1]] = parts[0]


pnMap = {}
pnEntries = [ line.strip() for line in open(pnMapFileName) ]
for entry in pnEntries:
    entry = entry[6:] # remove PNMap(
    entry = entry[:-2] # remove ).
    parts = entry.split(',')
    pnMap[parts[0]] = parts[1]


methodMap = {}
methodEntries = [ line.strip() for line in open(methodMapFileName) ]
for entry in methodEntries:
    parts = entry.split(':')
    parts[1] = parts[1].split('(')[0]  #  SRK Temp fix until method args are fixed.
    methodMap[parts[0]] = parts[1]


enclosingCatchMap = {}
enCEntries = [ line.strip() for line in open(enclosingCatchFileName) ]
for entry in enCEntries:
    entry = entry[12:] # remove EnclosingEH(
    entry = entry[:-2] # remove ).
    parts = entry.split(',')
    enclosingCatchMap[parts[2]] = parts[0]


########################################################################################################################
# Compute EDB probabilities 


probEdbEntries = [ line.strip() for line in open(probEdbFileName) ]
for entry in probEdbEntries:
    tup = entry
    entry = entry[11:] # remove CondCallAt(
    entry = entry[:-1] # remove )
    parts = entry.split(',')
    bnetNodeId = bnetDictMap[tup]
    condMethId = parts[0]
    condMeth = methodMap[condMethId]
    condPP = parts[1]
    condLoc = pnMap[condPP]
    callerId = parts[2]
    caller = methodMap[callerId]
    callerPP = parts[3]
    callerLoc = pnMap[callerPP]
    calleeId = parts[4]
    callee = methodMap[calleeId]

    totalCnt = 0
    callCnt = 0
    callerNodes = method2NodeMap[caller]
    for node in callerNodes:
        callerNdx = node.getIndex()
        if (condMeth in node.parent.data) and (condLoc == node.parent.offset[callerNdx]): # if the "condition" applies
            totalCnt += 1
            childNdx = node.getChild(callee)
            if (childNdx != -1) and (node.offset[childNdx] == callerLoc):
                callCnt += 1
    if (callCnt == 0) and (callerPP in enclosingCatchMap) and (enclosingCatchMap[callerPP] == callerId):
        prob = midProb
    elif totalCnt == 0:
        prob = minProb
    else:
        prob = minProb + ((maxProb - minProb) * callCnt / totalCnt)
    print("{0}: {1}".format(bnetNodeId, prob))
