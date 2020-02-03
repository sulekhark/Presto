#!/usr/bin/env python3


# find_application_methods.py 
# ASSUMPTION: This script executes in the datalog dir of the benchmark.

import logging
import re
import sys
import os


blacklistFileName = os.environ['PRESTO_HOME'] + "/DynamicConfig/scripts/blacklist.txt"
methodFileName = "M.map"
appMethodsDlogFileName = "appM.datalog" 
appMethodsTxtFileName = "appM.txt" 

logging.basicConfig(level=logging.INFO, \
                            format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                                                datefmt="%H:%M:%S")


########################################################################################################################
# Create a set of whitelisted method ids 

dlogFile = open(appMethodsDlogFileName, 'w')
txtFile = open(appMethodsTxtFileName, 'w')

blackListPrefixes = [ line.strip() for line in open(blacklistFileName) ]
methId = 0
for meth in open(methodFileName):
    meth = meth.strip()
    blacklist = False
    for pre in blackListPrefixes:
        if meth.startswith(pre):
            blacklist = True
            break
        elif "..ctor" in meth:
            blacklist = True
            break
    if blacklist == False:
        print("appM({0}).".format(methId), file=dlogFile)
        print("appM({0}:{1}).".format(methId, meth), file=txtFile)
    methId += 1

dlogFile.close()
txtFile.close()
