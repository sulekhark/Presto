#!/usr/bin/env python3

# We need to parse the file FInject.txt 

# parse_finject.py finjectFileName outFileName dirFileName

import logging
import re
import sys


inputFileName = sys.argv[1]
outFileName = sys.argv[2]
dirFileName = sys.argv[3]

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

dirNamePrefix = 'T'
separator = '@'

########################################################################################################################
# Parse input 

outFile = open(outFileName, 'w') 
dirFile = open(dirFileName, 'w')

for inputStr in open(inputFileName):
    inputStr = inputStr[8:]  # Remove "FInject("
    part1 = inputStr.split(' CLASS:',1)[0].strip()
    rest1 = inputStr.split(' CLASS:',1)[1]

    part2 = rest1.split(',',2)[1].strip()
    rest2 = rest1.split(',',2)[2]

    part3 = rest2.split(' CLASS:',1)[0].strip()
    rest3 = rest2.split(' CLASS:',1)[1]

    rest4 = rest3.split(',',1)[1]
    part4 = rest4.split(' MODULE:',1)[0].strip()

    id1 = part1.split(':',1)[0]
    callerMethod = part1.split(':',1)[1].strip()

    id2 = part2.split(':',1)[0]
    ilOffset = part2.split(':',1)[1].strip()

    id3 = part3.split(':',1)[0]
    calleeMethod = part3.split(':',1)[1].strip()

    id4 = part4.split(':',1)[0]
    exceptionType = part4.split(':',1)[1].strip()

    dirName = dirNamePrefix + "_" + id1 + "_" + id2 + "_" + id3 + "_" + id4

    outStrings = []
    outStrings.append(dirName)
    outStrings.append(callerMethod)
    outStrings.append(ilOffset)
    outStrings.append(calleeMethod)
    outStrings.append(exceptionType)
    print("{0}".format(separator.join(outStrings)), file=outFile)
    print("{0}".format(dirName), file=dirFile)

outFile.close()
dirFile.close()
