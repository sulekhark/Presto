#!/usr/bin/env python3

# get_edbs_to_refine.py counts.txt app_prob_edbs_to_be_refined.txt app_prob_edbs_no_refine.txt
# ASSUMPTION: This script executes in the bnet directory.

import logging
import re
import sys

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

countFileName = sys.argv[1]
refineFileName = sys.argv[2]
noRefineFileName = sys.argv[3]
 
methodMapFileName = "../dynconfig/id_to_method_map.txt"

# Later, if required, refineLimit can be read from presto_cfg.sh
refineLimit = 100

methodMap = {}
methodEntries = [ line.strip() for line in open(methodMapFileName) ]
for entry in methodEntries:
    parts = entry.split(':')
    # parts[1] = parts[1].split('(')[0]  #  SRK Temp fix until method args are fixed.
    methodMap[parts[0]] = parts[1]


########################################################################################################################

def isRefinable(tup):
    if tup.startswith("CallAt"):
        tup = tup[7:] # remove CallAt(
        tup = tup[:-1] # remove )
        parts = tup.split(',')
        callerId = parts[0]
        caller = methodMap[callerId]
        if ".MoveNext()" in caller:
            return False
        else:
            return True
    elif tup.startswith("EscapeMTP"):
        return True
    return False


refineFile = open(refineFileName, 'w')
noRefineFile = open(noRefineFileName, 'w')

numSelected = 0
for line in open(countFileName):
    line = line.strip()
    tup = line.split(' ')[0]
    if isRefinable(tup) and numSelected < refineLimit:
        print(tup, file=refineFile)
        numSelected += 1
    else:
        print(tup, file=noRefineFile)

refineFile.close()
noRefineFile.close()
