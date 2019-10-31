#!/usr/bin/env python3

import logging
import re
import sys
import os

# ASSUMPTIONS about parsing the instrumented datalog file
#  1. For every output rel, the number of its arguments need to be specified as follows:
#     # ARG_COUNT InstFldRead 3  <--- ARG_COUNT is like a keyword.
#     When this string is split on ' ', index 2 is relation name and index 3 is count.
#     All such ARG_COUNT lines must occur before the instrumented constraints.
#
#  2. The part preceding '_' in an instrumented relation name, is the original relation name.
#
#  3. In the RHS of a constraint, the antecedent facts are separated by ', ' (comma, space)
#
#  4. Within a fact (i.e. a tuple), there is no space. Arguments are separated by comma.
#
#  5. No gap between '!' and relation name for negated antecedent facts.

instrumentedAnalFileName = sys.argv[1]
dgraphFileName = sys.argv[2]
dgraphFile = open(dgraphFileName, 'w')

marker = '@'
relNameToArgCount = {}

####################################################################
# Utility Functions

def isIncludeDom(line):
    if ('.include' in line and '.dom' in line):
        return True
    return False 


def isIncludeRel(line):
    if ('.include' in line and '.datalog' in line):
        return True
    return False 


def isInputRel(line):
    if (' input' in line):
        return True
    return False 

def isOutputRel(line):
    if (' printtuples' in line):
        return True
    return False 


def isConstraint(line):
    if (' :- ' in line):
        return True
    return False 


def isArgCount(line):
    if ('# ARG_COUNT ' in line):
        return True
    return False 


def parseArgCount(line):
    relName = line.split(' ')[2]
    argCount = int(line.split(' ')[3])
    relNameToArgCount[relName] = argCount


####################################################################
# The processing of each instrumented constraint:
# The global variables and data structures in this part get
# freshly populated for each constraint.

instrumentedRelArgCnt = 0
instrumentedRelArgs = []
relFileName = ''
templateStr = ''

def processLHS(lhs):
    global relFileName
    global instrumentedRelArgCnt
    global instrumentedRelArgs
    global marker
    relFileName = lhs.split('(',1)[0] + '.datalog'
    argList = lhs.split('(',1)[1]
    argList = argList[:-1]
    instrumentedRelArgs = argList.split(',')
    instrumentedRelArgCnt = len(instrumentedRelArgs)
    for i in range(0, instrumentedRelArgCnt):
        instrumentedRelArgs[i] = marker + instrumentedRelArgs[i] + marker 
    return


def convertAntecedent(relTuple):
    global marker
    if (relTuple.startswith('!')):
        retStr = ''
        relTuple = relTuple[1:]
    else:
        retStr = 'NOT '
    rName = relTuple.split('(',1)[0]
    argStr = relTuple.split('(',1)[1]
    argStr = argStr[:-1] # remove trailing ')'
    args = argStr.split(',')
    retStr = retStr + rName + '('
    for i in range (0, len(args)):
        if (args[i].isdigit()): # if arg is a constant, no need to replace.
            retStr = retStr + args[i] + ','
        else:
            retStr = retStr + marker + args[i] + marker + ','
    retStr = retStr[:-1] # remove last comma
    retStr = retStr + ')'
    return retStr
 

def convertConsequent(relTuple):
    global marker
    retStr = ''
    rName = relTuple.split('_',1)[0]
    argStr = relTuple.split('(',1)[1]
    argStr = argStr[:-1] # remove trailing ')'
    args = argStr.split(',')
    retStr = retStr + rName + '('
    for i in range (0, relNameToArgCount[rName]):
        if (args[i].isdigit()): # if arg is a constant, no need to replace.
            retStr = retStr + args[i] + ','
        else:
            retStr = retStr + marker + args[i] + marker + ','
    retStr = retStr[:-1] # remove last comma
    retStr = retStr + ')'
    return retStr



def getTemplate(lhs, rhs):
    global templateStr
    processLHS(lhs)
    rhs = rhs[:-1] # remove the trailing dot.
    parts = rhs.split(', ') # get the terms of the rhs
    for part in parts:
        templateStr = templateStr + convertAntecedent(part) + ', '
    templateStr = templateStr + convertConsequent(lhs) 
    return


def getReplaceDict(line):
    global instrumentedRelArgCnt
    global instrumentedRelArgs
    instArgStr = line.split('(',1)[1]
    instArgStr = instArgStr[:-2] # remove trailing ').'
    instArgs = instArgStr.split(',')
    replDict = {}
    for i in range(0, instrumentedRelArgCnt):
        replDict[instrumentedRelArgs[i]] = instArgs[i]
    return replDict 


def processInstantiations():
    global relFileName
    global templateStr
    global dgraphFile
    for line in open(relFileName):
        line = line.strip()
        replaceDict = getReplaceDict(line)
        instStr = templateStr       
        for var,value in replaceDict.items():
            instStr = instStr.replace(var, value)
        print(instStr, file=dgraphFile)
    return


def processConstraint(line):
    parts = line.split(':-')
    lhs = parts[0].strip()
    rhs = parts[1].strip() 
    getTemplate(lhs, rhs)
    processInstantiations()
    print(relFileName)
    print(instrumentedRelArgCnt)
    print(instrumentedRelArgs)
    print(templateStr)
    print('')
    print('')
    return


def reset():
    global relFileName
    global instrumentedRelArgCnt
    global instrumentedRelArgs
    global templateStr
    relFileName = ''
    instrumentedRelArgCnt = 0
    instrumentedRelArgs = []
    templateStr = ''
    return


####################################################################    
# Main Program

for line in open(instrumentedAnalFileName):
    line = line.strip()
    if isArgCount(line):
        parseArgCount(line)
    elif isConstraint(line):
        reset()
        processConstraint(line)
dgraphFile.close()

####################################################################    
