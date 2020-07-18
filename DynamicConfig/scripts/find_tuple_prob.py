#!/usr/bin/env python3

# We need to parse torch logs and extract the call-tree from the logs.
# We will then compute the probabilities by using the call-tree to count conditional method calls.

# find_tuple_prob.py probEdbFileName bnetDictFileName > outFileName
#
# ASSUMPTION: This script executes in the bnet dir of the benchmark.
# ASUMPTION: This script is invoked separately once for CallAt/ConCallAt tuples and once more for 
#            EscapeMTP/CondEscapeMTP/LinkedEx tuples

import logging
import re
import sys
import os, fnmatch


probEdbFileName = sys.argv[1]
bnetDictFileName = sys.argv[2]

mmFileName = "../datalog/MM.datalog"
addrTakenFileName = "../datalog/MAddrTakenFunc.datalog"
pnMapFileName = "../datalog/PNMap.datalog"
methodMapFileName = "../dynconfig/id_to_method_map.txt"
enclosingCatchFileName = "../datalog/EnclosingEH.datalog"
delegateCallFileName = "../datalog/DelegateCall.datalog"
excMapFileName = "../dynconfig/id_to_exctype_map.txt"
loggingDir = "../dynlogs/Logging"
fInjectDir = "../dynlogs/FaultInjectionSet/FInject"
linkInjectDir = "../dynlogs/FaultInjectionSet/LinkInject"
logDirPrefix = "T"

minProb = 0.05
maxProb = 0.98
midProb = 0.94
scaler = 4
scalingOn = True

logging.basicConfig(level=logging.INFO, \
                            format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                                                datefmt="%H:%M:%S")


########################################################################################################################
# Define the tree data structure for call tree 

from tree import Node 

########################################################################################################################
# Functions to read in the log files 

def insertIntoMap(methName, node):
    # methName = methName.split('(')[0]   # SRK temp hack until method names are fixed
    if methName not in method2NodeMap:
        method2NodeMap[methName] = []
    method2NodeMap[methName].append(node)
    return


def getDelegateCallerAnc(delMeth, crntNode):
    if delMeth in delegateCallMap:
        delCallerList = delegateCallMap[delMeth]
        if len(delCallerList) > 0:
            for pr in delCallerList:
                delCaller = pr[0]
                anc = crntNode.getAncestor(delCaller)
                if anc is not None:
                    return tuple([anc, pr[1]])
    return None


def getDelegateCallerAny(delMeth):
    if delMeth in delegateCallMap:
        delCallerList = delegateCallMap[delMeth]
        if len(delCallerList) > 0:
            for pr in delCallerList:
                delCaller = pr[0]
                if delCaller in method2NodeMap:
                    anc = method2NodeMap[delCaller]
                    return tuple([anc[-1], pr[1]])
    return None


def readLogFiles(logFNList):
    for logFileName in logFNList:
        logLines = [line.strip() for line in open(logFileName) ]
        logLines = logLines[1:]
        crntNode = None
        for line in logLines:
            lineParts = line.split(';')
            caller = lineParts[6]
            callee = lineParts[7]
            ilOffset = lineParts[10]
            exception = lineParts[14]
            if "..ctor" in caller:      # Ignore log lines that are calls from constructors
                continue
            if ".Invoke(" in callee:    # Ignore calls to Delegate Invoke becuase DAFFODIL is modeling delegate invoke.
                continue
            if (crntNode is None):   # Starting the call tree
                node = Node(data=caller)
                insertIntoMap(caller, node)
                crntNode = node
                logRoots.append(node)
            elif crntNode.data != caller:  # there is a disconnect
                detached = True
                ancestor = crntNode.getAncestor(caller)
                if ancestor is not None:   # found some ancestor
                    detached = False
                    crntNode = ancestor
                else: # No ancestor - check if there is a delegate caller among the ancestors
                    ancPair = getDelegateCallerAnc(caller, crntNode)
                    if ancPair is not None:  # found a delegate caller among ancestors
                        detached = False
                        node = Node(data=caller)
                        insertIntoMap(caller, node)
                        ancPair[0].addChild(node, ancPair[1], "")
                        crntNode = node
                if detached:  # no ancestor and no delagate caller among ancestors
                    if caller in method2NodeMap:  # found caller among previously processed methods
                        lst = method2NodeMap[caller]
                        crntNode = lst[-1]
                    else: # caller not among previously processed methods - check if there is a delegate caller among them
                        ancPair = getDelegateCallerAny(caller)
                        if ancPair is not None:  # found delegate caller among previously processed methods
                            node = Node(data=caller)
                            insertIntoMap(caller, node)
                            ancPair[0].addChild(node, ancPair[1], "")
                            crntNode = node
                        else:  # no previously processed method or delegate caller among them - create a new root
                            node = Node(data=caller)
                            insertIntoMap(caller, node)
                            crntNode = node
                            logRoots.append(node)

            calleeNode = Node(data=callee)
            insertIntoMap(callee, calleeNode)
            crntNode.addChild(calleeNode, ilOffset, exception)
            crntNode = calleeNode
    # for root in logRoots:
        # root.printTree()
    return


########################################################################################################################
# Functions to get relevant log files and other helper functions

def getTorchLogs(dirName):
    innerDirNames = [fn for fn in os.listdir(dirName)]
    logs = []
    for innerDir in innerDirNames:
        idn = os.path.join(dirName, innerDir)
        tn = [os.path.join(idn, fn) for fn in os.listdir(idn) if fn.startswith('torch')]
        assert len(tn) == 1
        logs.append(tn[0])
    return logs


def getLogDirs(containingDir, dirNameSelector):
    dirNames = fnmatch.filter(os.listdir(containingDir), dirNameSelector)
    dirNames = [os.path.join(containingDir, dn) for dn in dirNames]
    return dirNames


def getLogs(dirName):
    innerDirNames = [fn for fn in os.listdir(dirName)]
    logs = []
    for innerDir in innerDirNames:
        idn = os.path.join(dirName, innerDir)
        tn = [os.path.join(idn, fn) for fn in os.listdir(idn) if fn.startswith('torch')]
        assert len(tn) == 1
        en = [os.path.join(idn, fn) for fn in os.listdir(idn) if fn.startswith('execution')]
        assert len(en) == 1
        logs.append(tuple([tn[0], en[0]]))
    return logs


def getCallCount(callerMeth, calleeMeth, callerLoc):
    callCnt = 0
    if callerMeth in method2NodeMap:
        callerNodes = method2NodeMap[callerMeth]
        for node in callerNodes:
            calleeNdx = node.getChildAtOffset(calleeMeth, callerLoc)
            if (calleeNdx > -1):
                callCnt += 1
    return callCnt


def modifyPropertyName(meth):
    if "get_" in meth:
        meth = meth.replace("get_", "")
        meth = meth.replace("(", ".get(")
    return meth


def modifyCaller(meth):
    meth = modifyPropertyName(meth)
    if ".MoveNext(" in meth:          # async method
        return moveNextCallMap[moveNextCallMap[meth]]
    elif ("<>c__Display" in meth) and (">b__" in meth):  # anonymous method
        return anonFuncContainerMap[meth]
    return meth

def modifyCallee(meth):
    meth = modifyPropertyName(meth)
    return meth



########################################################################################################################
# Function to compute CallAt probabilities 

def computeProbCallAt(entry): 
    bnetNodeId = bnetDictMap[entry]
    entry = entry[7:] # remove CallAt(
    entry = entry[:-1] # remove )
    parts = entry.split(',')
    callerId = parts[0]
    caller = methodMap[callerId]
    callerPP = parts[1]
    callerLoc = pnMap[callerPP]
    calleeId = parts[2]
    callee = methodMap[calleeId]

    caller = modifyCaller(caller)
    callee = modifyCallee(callee)

    totalCnt = 0
    if caller in method2NodeMap:
        totalCnt = len(method2NodeMap[caller])
    callCnt = getCallCount(caller, callee, callerLoc)

    if totalCnt == 0:
        prob = maxProb
    elif (callCnt == 0) and (callerPP in enclosingCatchMap) and (enclosingCatchMap[callerPP] == callerId):
        prob = midProb
    elif (callCnt == 0):
        prob = minProb
    else:
        ratio = callCnt * 1.0 / totalCnt
        if scalingOn == True:
            scaleFactor = ratio ** (1.0 / scaler)
        else:
            scaleFactor = ratio
        prob = minProb + ((maxProb - minProb) * scaleFactor)
    # logging.info('SRK_DBG: find_tuple_prob.py: tuple:{0} {1}  totalCnt:{2}  callCnt:{3} prob: {4}'.format(caller, callee, totalCnt, callCnt, prob))
    print("{0}: {1}".format(bnetNodeId, prob))
    return


########################################################################################################################
# Function to compute CondCallAt probabilities 

def computeProbCondCallAt(entry): 
    bnetNodeId = bnetDictMap[entry]
    entry = entry[11:] # remove CondCallAt(
    entry = entry[:-1] # remove )
    parts = entry.split(',')
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

    condMeth = modifyCaller(condMeth)
    caller = modifyCaller(caller)
    callee = modifyCallee(callee)

    totalCnt = 0
    callCnt = 0
    if caller in method2NodeMap:
        callerNodes = method2NodeMap[caller]
        for node in callerNodes:
            callerNdx = node.getIndex()
            if (condMeth in node.parent.data) and (condLoc == node.parent.offset[callerNdx]): # if the "condition" applies
                totalCnt += 1
                childNdx = node.getChildAtOffset(callee, callerLoc)
                if childNdx > -1:
                    callCnt += 1
    if (totalCnt == 0) and (condPP in enclosingCatchMap) and (enclosingCatchMap[condPP] == condMethId):
        prob = midProb
    elif totalCnt == 0:
        prob = maxProb
    elif (callCnt == 0) and (callerPP in enclosingCatchMap) and (enclosingCatchMap[callerPP] == callerId):
        prob = midProb
    elif (callCnt == 0):
        prob = minProb
    else:
        ratio = callCnt * 1.0 / totalCnt
        if scalingOn == True:
            scaleFactor = ratio ** (1.0 / scaler)
        else:
            scaleFactor = ratio
        prob = minProb + ((maxProb - minProb) * scaleFactor)
    print("{0}: {1}".format(bnetNodeId, prob))
    return


########################################################################################################################
# Function to compute EscapeMTP and CondEscapeMTP probabilities 

def getExcThrownCount(callerMeth, calleeMeth, callerLoc, excType):
    excCnt = 0
    if callerMeth in method2NodeMap:
        callerNodes = method2NodeMap[callerMeth]
        for node in callerNodes:
            calleeNdx = node.getChildAtOffset(calleeMeth, callerLoc)
            if (calleeNdx > -1) and (node.exception[calleeNdx] == excType):
                    excCnt += 1
    return excCnt


def getExcThrownCountFromOutput(execLog, excType):
    for line in open(execLog):
        if ("Unhandled Exception:" in line) and (excType in line):
            return 1
    return 0


def computeProbEscapeMTP(entry): 
    bnetNodeId = bnetDictMap[entry]
    entry = entry[:-1] # remove )
    isConditional = False
    if entry.startswith('EscapeMTP'):
        entry = entry[10:] # remove EscapeMTP(
    elif entry.startswith('CondEscapeMTP'):
        isConditional = True
        entry = entry[14:] # remove CondEscapeMTP(
        cparts = entry.split(',', 1)
        calleeMethId = cparts[0]
        calleeMeth = methodMap[calleeMethId]
        calleeMeth = modifyCallee(calleeMeth)
        entry = cparts[1]

    parts = entry.split(',')
    callerMethId = parts[0]
    callerMeth = methodMap[callerMethId]
    callerMeth = modifyCaller(callerMeth)
    excTypeId = parts[1]
    excType = excMap[excTypeId]
    callerPP = parts[2]

    if callerPP in pnMap:
        callerLoc = pnMap[callerPP]
        if isConditional == True:
            selector = calleeMethId
        else:
            selector = '*'
        logDirName = logDirPrefix + "_" + callerMethId + "_" + callerPP + "_" + selector + "_" + excTypeId
        logDirList = getLogDirs(fInjectDir, logDirName)
        callCountTotal = 0
        excCountTotal = 0
        for lDir in logDirList:
            if isConditional == False:
                calleeMethId = lDir.split('_')[3]
                calleeMeth = methodMap[calleeMethId]
                calleeMeth = modifyCallee(calleeMeth)
            fInjectLogs = getLogs(lDir)
            for fiLogPair in fInjectLogs:
                torchLog = fiLogPair[0]
                execLog = fiLogPair[1]
                logRoots.clear()
                method2NodeMap.clear()
                readLogFiles([torchLog])
                callCount = getCallCount(callerMeth, calleeMeth, callerLoc)
                if callCount > 0:
                    if ".Main(" in callerMeth:
                        excCount = getExcThrownCountFromOutput(execLog, excType)
                    else:
                        excCount = getExcThrownCount(callerMeth, calleeMeth, callerLoc, excType)
                    callCountTotal += callCount
                    excCountTotal += excCount
        if (callCountTotal == 0) and (callerPP in enclosingCatchMap) and (enclosingCatchMap[callerPP] == callerMethId):
            prob = midProb
        elif callCountTotal == 0:
            prob = maxProb
        elif callCountTotal > 0 and excCountTotal == 0:
            prob = minProb
        else:
            ratio = excCountTotal * 1.0 / callCountTotal
            if scalingOn == True:
                scaleFactor = ratio ** (1.0 / scaler)
            else:
                scaleFactor = ratio
            prob = minProb + ((maxProb - minProb) * scaleFactor)
    else: # We cannot get a probability from Torch logs for this tuple
        prob = maxProb
    print("{0}: {1}".format(bnetNodeId, prob))
    return


########################################################################################################################
# Function to compute LinkedEx probabilities 

def computeProbLinkedEx(entry):
    bnetNodeId = bnetDictMap[entry]
    entry = entry[:-1] # remove )
    entry = entry[9:] # remove LinkedEx(
    parts = entry.split(',')
    throwMethId = parts[3] # throwMeth is the method that contains the "throw" statement that is nested in the catch block that it is linked with.
    throwMeth = methodMap[throwMethId]
    throwMeth = modifyCaller(throwMeth)  # throwMeth is treated like a "caller" for modification because it has to be an app meth.
    excTypeId = parts[5]
    excType = excMap[excTypeId]
    arg0 = parts[0]
    arg1 = parts[1]
    arg2 = parts[2]
    arg4 = parts[4]

    logDirName = logDirPrefix + "_" + arg0 + "_" + arg1 + "_" + arg2 + "_" + throwMethId + "_" + arg4 + "_" + excTypeId
    logDirList = getLogDirs(linkInjectDir, logDirName)
    assert len(logDirList) == 1
    logDir = logDirList[0]
    linkInjectLogs = getLogs(logDir)

    callCount = 0
    excCount = 0
    for liLogPair in linkInjectLogs:
        torchLog = liLogPair[0]
        execLog = liLogPair[1]
        logRoots.clear()
        method2NodeMap.clear()
        readLogFiles([torchLog])
        if throwMeth in method2NodeMap:
            throwNodes = method2NodeMap[throwMeth]
            callCount += len(throwNodes)
            for node in throwNodes:
                nodeNdx = node.getIndex()
                if (nodeNdx >= 0) and (node.parent.exception[nodeNdx] == excType):
                    excCount += 1
                elif nodeNdx < 0:
                    if ".Main(" in throwMeth:
                        excCount = getExcThrownCountFromOutput(execLog, excType)
    if callCount == 0:
        prob = maxProb
    elif excCount == 0:
        prob = minProb
    else:
        ratio = excCount * 1.0 / callCount
        if scalingOn == True:
            scaleFactor = ratio ** (1.0 / scaler)
        else:
            scaleFactor = ratio
        prob = minProb + ((maxProb - minProb) * scaleFactor)
    print("{0}: {1}".format(bnetNodeId, prob))
    # logging.info('SRK_DBG: LINKED_EX INFO: throwMeth:{0}  excCnt:{1}  callCnt:{2}'.format(throwMeth, excCount, callCount))
    return


########################################################################################################################
# Read in the various map files 

bnetDictMap = {}
bnetDictEntries = [ line.strip() for line in open(bnetDictFileName) ]
for entry in bnetDictEntries:
    if 'NOT' not in entry:  # we need only the bnet map entries for tuples not for rules.
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
    # parts[1] = parts[1].split('(')[0]  #  SRK Temp fix until method args are fixed.
    methodMap[parts[0]] = parts[1]


enclosingCatchMap = {}
enCEntries = [ line.strip() for line in open(enclosingCatchFileName) ]
for entry in enCEntries:
    entry = entry[12:] # remove EnclosingEH(
    entry = entry[:-2] # remove ).
    parts = entry.split(',')
    enclosingCatchMap[parts[2]] = parts[0]


excMap = {}
excEntries = [ line.strip() for line in open(excMapFileName) ]
for entry in excEntries:
    parts = entry.split(':')
    excMap[parts[0]] = parts[1]


delegateCallMap = {}
delCEntries = [ line.strip() for line in open(delegateCallFileName) ]
for entry in delCEntries:
    entry = entry[13:] # remove DelegateCall(
    entry = entry[:-2] # remove ).
    parts = entry.split(',')
    delMeth = methodMap[parts[2]]
    if delMeth not in delegateCallMap:
        delegateCallMap[delMeth] = []
    delegateCallMap[delMeth].append(tuple([methodMap[parts[0]], parts[1]]))


asyncMethodSet = set()
moveNextCallMap = {}
mmEntries = [ line.strip() for line in open(mmFileName) ]
for entry in mmEntries:
    entry = entry[3:] # remove MM( 
    entry = entry[:-2] # remove ).
    parts = entry.split(',')
    callerMeth = methodMap[parts[0]]
    calleeMeth = methodMap[parts[1]]
    if ".MoveNext(" in calleeMeth:
        moveNextCallMap[calleeMeth] = callerMeth 
valueSet = set(moveNextCallMap.values())
for entry in mmEntries:
    entry = entry[3:] # remove MM( 
    entry = entry[:-2] # remove ).
    parts = entry.split(',')
    callerMeth = methodMap[parts[0]]
    calleeMeth = methodMap[parts[1]]
    if calleeMeth in valueSet:
        moveNextCallMap[calleeMeth] = callerMeth 
        asyncMethodSet.add(callerMeth)


anonFuncContainerMap = {}
atEntries = [ line.strip() for line in open(addrTakenFileName) ]
for entry in atEntries:
    entry = entry[15:] # remove MAddrTakenFunc(
    entry = entry[:-2] # remove ).
    parts = entry.split(',')
    atMeth = methodMap[parts[2]]
    if ("<>c__Display" in atMeth) and (">b__" in atMeth):  # anonymous method
        anonFuncContainerMap[atMeth] = methodMap[parts[0]]


########################################################################################################################
# Compute probabilities 

logRoots = []
method2NodeMap = {}
probEdbEntries = [ line.strip() for line in open(probEdbFileName) ]

if 'CallAt' in probEdbEntries[0]:
    logFileNameList = getTorchLogs(loggingDir)
    readLogFiles(logFileNameList)

for entry in probEdbEntries:
    if 'CondCallAt' in entry:
        computeProbCondCallAt(entry)
    elif 'CallAt' in entry:
        computeProbCallAt(entry)
    elif ('EscapeMTP' in entry): # CondEscapeMTP is also handled in the same function.
        computeProbEscapeMTP(entry)
    elif ('LinkedEx' in entry):
        computeProbLinkedEx(entry)
