#!/usr/bin/env python3

# We need to parse the file FInject.txt 

# parse_linkinject.py linkinjectFileName outFileName dirFileName

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
separator1 = '@'
separator2 = '#'

########################################################################################################################
# Parse input 

outFile = open(outFileName, 'w') 
dirFile = open(dirFileName, 'w')
outStrings = {}

for inputStr in open(inputFileName):
    inputStr = inputStr[11:]  # Remove "LinkInject("
    part1 = inputStr.split(' CLASS:',1)[0].strip()
    part1_clean = part1
    rest1 = inputStr.split(' CLASS:',1)[1]

    part2 = rest1.split(' CLASS:',1)[0].strip()
    part2_clean = part2.split(',',1)[1]
    rest2 = rest1.split(' CLASS:',1)[1]

    part3 = rest2.split(' MODULE:',1)[0].strip()
    part3_clean = part3.split('),',1)[1]
    rest3 = rest2.split(' MODULE:',1)[1]

    part4 = rest3.split(' CLASS:',1)[0].strip()
    part4_clean = part4.split(',',1)[1]
    rest4 = rest3.split(' CLASS:',1)[1]

    part5 = rest4.split(' CLASS:',1)[0].strip()
    part5_clean = part5.split(',',1)[1]
    rest5 = rest4.split(' CLASS:',1)[1]

    part6 = rest5.split(' MODULE:',1)[0].strip()
    part6_clean = part6.split('),',1)[1]
    rest6 = rest5.split(' MODULE:',1)[1]

    part7 = rest6.split(' CLASS:',1)[0].strip()
    part7_clean = part7.split(',',1)[1]
    part8 = rest6.split(' CLASS:',1)[1].strip()
    part8_clean_1 = part8.split(',',1)[1]
    part8_clean = part8_clean_1.split(').',1)[0]

    id1 = part1_clean.split(':',1)[0]
    callerMethod = part1_clean.split(':',1)[1].strip()

    id2 = part2_clean.split(':',1)[0]

    id3 = part3_clean.split(':',1)[0]
    exceptionType = part3_clean.split(':',1)[1].strip()

    id4 = part4_clean.split(':',1)[0]
    id5 = part5_clean.split(':',1)[0]
    id6 = part6_clean.split(':',1)[0]

    id7 = part7_clean.split(':',1)[0]
    calleeMethod = part7_clean.split(':',1)[1].strip()

    id8 = part8_clean.split(':',1)[0]
    ilOffset = part8_clean.split(':',1)[1].strip()

    dirName = dirNamePrefix + "_" + id1 + "_" + id2 + "_" + id3 + "_" + id4 + "_" + id5 + "_" + id6

    # print("{0}   {1}    {2}".format(part1_clean, part2_clean, part3_clean))
    # print("{0}   {1}    {2}".format(part4_clean, part5_clean, part6_clean))
    # print("{0}   {1}".format(part7_clean, part8_clean))
    # print("{0}".format(dirName))
    # print("")

    calleeString = ilOffset + ":" + calleeMethod
    if not dirName in outStrings:
        val = dirName + separator1 + callerMethod + separator1 + exceptionType + separator1 + calleeString
        outStrings[dirName] = val
    else:
        val = outStrings[dirName]
        val = val + separator2 + calleeString
        outStrings[dirName] = val

for dirName, dirLine in outStrings.items():
    print("{0}".format(dirLine), file=outFile)
    print("{0}".format(dirName), file=dirFile)
outFile.close()
dirFile.close()
