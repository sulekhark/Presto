#!/usr/bin/env python3

# ./htmlize.py metadata_dir root_idb.txt root_headers.txt display_desc.txt bnet_dict.out all_probabilities.txt alarm_ranking.txt < constraints.txt

import logging
import re
import sys
import os

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

metadataDir = sys.argv[1]
rootIdbFileName = sys.argv[2]
rootHeadersFileName = sys.argv[3]
displayDescFileName = sys.argv[4]
bnetDictFileName = sys.argv[5]
allProbsFileName = sys.argv[6]
alarmRankingFileName = sys.argv[7]
 
refineInfo = os.environ['REFINE_INFO']

inputDefnFileName = metadataDir + "/input_defn.datalog"
domMapExtn = ".out"
ts = "  "  # tabspace in output html
lineBrk = "<br>"

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


def IsNarrowOrClause(cstr):
    parts = cstr.split(', ')
    conseq = parts[-1]
    if '(' in conseq:
        return False
    else:
        return True


def RemoveConsequent(cstr):
    parts = cstr.split(', ')
    return ', '.join(parts[:-1])

 
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
# 2. Create map: consequent to all producing clauses.

producingClauses = {}

for clause in allClauses:
    consequent = clause2Consequent(clause)
    if consequent not in producingClauses:
        producers = []
        producingClauses[consequent] = producers
    else:
        producers = producingClauses[consequent]
    producers.append(clause)


########################################################################################################################
# 3. Record the signatures of all (required) relation names. Create maps for the required DOM entries. 
#    Read in the display description.

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


displayDesc = {}
for line in open(displayDescFileName):
    line = line.strip()
    parts = line.split(':')
    displayDesc[parts[0]] = parts[1]


rootHeaders = {}
for line in open(rootHeadersFileName):
    line = line.strip()
    parts = line.split(':')
    rootHeaders[parts[0]] = parts[1]


########################################################################################################################
# 4. Read in bnet_dict.out and probabilities of all the bnet nodes.

bnetDictMap = {}
for line in open(bnetDictFileName):
    line = line.strip()
    parts = line.split(': ')
    clauseStr = parts[1]
    if IsNarrowOrClause(clauseStr):
        clauseStr = RemoveConsequent(clauseStr)
    bnetDictMap[clauseStr] = parts[0]

allProbsMap = {}
for line in open(allProbsFileName):
    line = line.strip()
    parts = line.split(': ')
    allProbsMap[parts[0]] = parts[1]


########################################################################################################################
# 5. Helper functions to generate the html pages

def getHtmlFileName(tup):
    tup = tup.replace('(', '_')
    tup = tup.replace(',', '_')
    tup = tup.replace(')', '')
    fn = tup + ".html"
    return fn 
    

def getRuleProb(clause):
    prob = "NA"
    cl = ', '.join(clause)
    if cl not in bnetDictMap:
        cl = RemoveConsequent(cl)
    if cl in bnetDictMap:
        bnetId = bnetDictMap[cl]
        if bnetId in allProbsMap:
            prob = allProbsMap[bnetId]
    return prob


def getTupleProb(tup):
    prob = "NA"
    if tup in bnetDictMap:
        bnetId = bnetDictMap[tup]
        if bnetId in allProbsMap:
            prob = allProbsMap[bnetId]
    return prob


def getTupleProbFloat(tup):
    prob = "0.0"
    if tup in bnetDictMap:
        bnetId = bnetDictMap[tup]
        if bnetId in allProbsMap:
            prob = allProbsMap[bnetId]
    return float(prob)


def getTupleDomDesc(tup):
    relName = tup.split('(')[0]
    relSig = relSignature[relName]
    args = getArgs(tup)
    ndx = 0
    desc = ""
    for arg in args:
        desc = desc + domMaps[relSig[ndx]][arg] + lineBrk
        ndx += 1
    return desc


########################################################################################################################
# 6. Create the html pages.

htmlFile = None
indent = 0

def hprint(s):
    toPrint = indent * ts + s
    print('{0}'.format(toPrint), file=htmlFile)
    return


def writeDerivation(consequent):
    global indent
    global htmlFile
    htmlFileName = getHtmlFileName(consequent)
    htmlFile = open(htmlFileName, 'w')
    indent = 0
    hprint("<!DOCTYPE html>")
    hprint("<html>")
    indent += 1
    hprint("<head>")
    indent += 1
    hprint("<title>Presto derivation graph</title>")
    indent -= 1
    hprint("</head>")
    hprint("<body>")
    indent += 1
    hprint("<table border=\"1\" width=\"100%\">")
    writeDerivTableHeader(consequent)
    writeDerivTableBody(consequent)
    hprint("</table>")
    indent -= 1
    hprint("</body>")
    indent -= 1
    hprint("</html>")
    htmlFile.close()
    return


def writeDerivTableHeader(consequent):
    global indent
    indent += 1
    hprint("<thead>")
    indent += 1
    hprint("<tr>")
    indent += 1
    writeTupleDesc(consequent, False)
    indent -= 1
    hprint("</tr>")
    indent -= 1
    hprint("</thead>")
    indent -= 1
    return


def writeTupleDesc(tup, linkPresent):
    prob = getTupleProb(tup)
    probStr = "  PROB: " + prob
    relName = tup.split('(')[0]
    if relName in displayDesc:
        desc = displayDesc[relName]
    else:
        desc = ""

    domDesc = getTupleDomDesc(tup)
    pre = "<td colspan=\"1\">"
    post = "</td>"
    if linkPresent:
        linkName = getHtmlFileName(tup)
        pre1 = "<a href=" + linkName + ">"
    else:
        pre1 = "<a>"
    post1 = "</a>"
    line1 = pre1 + tup + post1 + probStr + "  " + desc + lineBrk
    txt = pre + line1 + domDesc + post
    hprint(txt)
    return


def writeDerivTableBody(consequent):
    global indent
    indent += 1
    hprint("<tbody>")
    producers = producingClauses[consequent]
    for clause in producers:
        writeRule(clause)
    hprint("</tbody>")
    indent -= 1
    return


def writeRule(clause):
    global indent
    indent += 1
    hprint("<tr>")
    indent += 1
    hprint("<td>")
    indent += 1
    hprint("<table border=\"1\" width=\"100%\">")
    indent += 1
    hprint("<tbody>")
    indent += 1
    hprint("<tr>")
    indent += 1
    writeRuleProb(clause)
    indent -= 1
    for ant in clause2Antecedents(clause):
        indent += 1
        if ant in allConsequents:
            writeTupleDesc(ant, True)
        else:
            writeTupleDesc(ant, False)
        indent -= 1
    hprint("</tr>")
    indent -= 1
    hprint("</tbody>")
    indent -= 1
    hprint("</table>")
    indent -= 1
    hprint("</td>")
    indent -= 1
    hprint("</tr>")
    indent -= 1
    return


def writeRuleProb(clause):
    prob = getRuleProb(clause)
    probStr = "PROB:" + lineBrk + prob
    txt = "<td width=\"5%\"> " + probStr + " </td>"
    hprint(txt)
    return

for consequent in producingClauses.keys():
    writeDerivation(consequent)


########################################################################################################################
# 7. Create the index html page.

def writeIndexPage():
    global indent
    global htmlFile
    htmlFileName = "index.html"
    htmlFile = open(htmlFileName, 'w')
    indent = 0
    hprint("<!DOCTYPE html>")
    hprint("<html>")
    indent += 1
    hprint("<head>")
    indent += 1
    hprint("<title>Presto: Ranking of Exception Bugs</title>")
    indent -= 1
    hprint("</head>")
    hprint("<body>")
    writeAlarmRanking()
    writeRoots()
    hprint("</body>")
    indent -= 1
    hprint("</html>")
    htmlFile.close()
    return


def writeAlarmRanking():
    global indent
    writeAlarmRankingHeader()
    emptyLine()
    writeAlarmRankingTable()
    emptyLine()
    emptyLine()
    return


def writeAlarmRankingHeader():
    global indent
    indent += 1
    hprint("<tr>")
    indent += 1
    desc = "ALARM RANKING TABLE"
    pre = "<td colspan=\"1\"><u><font size=\"5\" color=\"red\">"
    post = "</font></u></td>"
    txt = pre + desc + lineBrk + post
    hprint(txt)
    indent -= 1
    hprint("</tr>")
    indent -= 1
    return


def writeAlarmRankingTable():
    global indent
    indent += 1
    hprint("<table>")
    indent += 1
    hprint("<tr>")
    indent += 1
    hprint("<td colspan=\"1\"><font size=\"4\" color=\"red\"> Rank&nbsp;&nbsp;&nbsp;&nbsp; </font></td>")
    hprint("<td colspan=\"1\"><font size=\"4\" color=\"red\"> Confidence&nbsp;&nbsp;&nbsp;&nbsp; </font></td>")
    hprint("<td colspan=\"1\"><font size=\"4\" color=\"red\"> GroundTruth&nbsp;&nbsp;&nbsp;&nbsp; </font></td>")
    hprint("<td colspan=\"1\"><font size=\"4\" color=\"red\"> Tuple </font></td>")
    indent -= 1
    hprint("</tr>")
    indent -= 1

    for line in open(alarmRankingFileName):
        indent += 1
        hprint("<tr>")
        indent += 1
        line = line.strip()
        parts = line.split('\t')
        for i in range(0, len(parts) - 1):
            if (i == 2) and ("True" in parts[i]):
                pre = "<td colspan=\"1\"> <font color=\"red\">"
                post = "</font></td>"
            else:
                pre = "<td colspan=\"1\">"
                post = "</td>"
            hprint(pre + parts[i] + post)

        tup = parts[-1]
        linkName = getHtmlFileName(tup)
        pre1 = "<td colspan=\"1\"><a href=" + linkName + ">"
        post1 = "</a></td>"
        hprint(pre1 + tup + post1)
        indent -= 1
        hprint("</tr>")
        indent -= 1

    hprint("</table>")
    indent -= 1
    return


def writeRoots():
    global indent
    for root in rootIdbs.keys():
        writeRootTableHeader(root)
        emptyLine()
        writeRootTableBody(root)
        emptyLine()
        emptyLine()
    return


def writeRootTableHeader(root):
    global indent
    indent += 1
    hprint("<tr>")
    indent += 1
    if root in rootHeaders:
        desc = rootHeaders[root]
    else:
        desc = ""
    pre = "<td colspan=\"1\"><u><font size=\"5\" color=\"red\">"
    post = "</font></u></td>"
    txt = pre + desc + lineBrk + post
    hprint(txt)
    indent -= 1
    hprint("</tr>")
    indent -= 1
    return


def writeRootTableBody(root):
    global indent
    slst = sorted(rootIdbs[root], key=lambda tup: getTupleProbFloat(tup), reverse=True)
    for tup in slst:
        indent += 1
        hprint("<tr>")
        indent += 1
        writeTupleDesc(tup, True)
        indent -= 1
        hprint("</tr>")
        indent -= 1
        emptyLine()
    return


def emptyLine():
    global indent
    indent += 1
    hprint("<tr>")
    indent += 1
    hprint("<td><br></td>")
    indent -= 1
    hprint("</tr>")
    indent -= 1


writeIndexPage()
