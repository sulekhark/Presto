#!/usr/bin/env python3

# random_edb_prob.py bnet_dict.out < prob_edb_tuples.txt > edb_prob.txt 

import logging
import re
import sys
import random

bnetDictFileName = sys.argv[1]

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

########################################################################################################################

if (bnetDictFileName != ""):
    bnetRevDict = [ line.strip().split(': ') for line in open(bnetDictFileName) ]
    bnetRevDict = { line[1]: int(line[0]) for line in bnetRevDict }
else:
    bnetRevDict = {}

for line in sys.stdin:
    line = line.strip()
    bnetNodeNum = bnetRevDict[line] if (line in bnetRevDict) else -1 
    if bnetNodeNum != -1:
        print("{0}: {1}".format(bnetNodeNum, random.uniform(0.5, 1.0)))
