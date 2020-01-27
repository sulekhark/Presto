#!/usr/bin/env python3

# get_edbs_to_refine.py counts.txt app_prob_edbs_to_be_refined.txt app_prob_edbs_no_refine.txt

import logging
import re
import sys

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

countFileName = sys.argv[1]
refineFileName = sys.argv[2]
noRefineFileName = sys.argv[3]
 
# Later, if required, refineLimit and tuplesToBeRefined can be read from presto_cfg.sh
refineLimit = 100
tuplesToBeRefined = set()
tuplesToBeRefined.add('CallAt')
tuplesToBeRefined.add('EscapeMTP')

########################################################################################################################

def isRefinable(tup):
    for pre in tuplesToBeRefined:
        if tup.startswith(pre):
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
