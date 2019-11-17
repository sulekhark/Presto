#!/usr/bin/env python3

# driver_ranking.py bnet_dict.out factorGraph.fg base_queries.txt
#
# Accepts human-readable commands from stdin, and passes them to LibDAI/wrapper.cpp, thus acting as a convenient driver.
# Arguments:
# 1. Dictionary file for the bayesian network, named-dict.out, produced by cons_all2bnet.py. This is to translate
#    commands, such as "O racePairs_cs(428,913) true" to the format accepted by LibDAI/wrapper.cpp, such as
#    "O 38129 true".
# 2. Factor graph, factor-graph.fg
# 3. Base queries file, base_queries.txt. This need not be the full list of base queries produced by Chord, but could
#    instead be any subset of it, such as the alarms reported by the upper oracle.
# 4. The EDB tuples that need to be clamped to true. 

import logging
import subprocess
import sys
import time
import re

dictFileName = sys.argv[1]
fgFileName = sys.argv[2]
baseQueriesFileName = sys.argv[3]

# wrapperExecutable = './libdai/wrapper'
wrapperExecutable = '/home/sulekha/Error-Ranking/chord-fork/libdai/wrapper'

logging.basicConfig(level=logging.INFO, \
                    format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                    datefmt="%H:%M:%S")

########################################################################################################################
# 1. Setup

# 1a. Populate bayesian network node dictionary
bnetDict = {}
for line in open(dictFileName):
    line = line.strip()
    if len(line) == 0: continue
    components = [ c.strip() for c in line.split(': ') if len(c.strip()) > 0 ]
    assert len(components) == 2
    bnetDict[components[1]] = components[0]

# 1b. Initialize set of labelled tuples (to confirm that tuples are not being relabelled), and populate the set of
# alarms in the ground truth.
labelledTuples = {}

baseQueries = set([ line.strip() for line in open(baseQueriesFileName) if len(line.strip()) > 0 ])
assert(baseQueries.issubset(set(bnetDict.keys())))

logging.info('Populated {0} base queries.'.format(len(baseQueries)))


########################################################################################################################
# 2. Start LibDAI/wrapper.cpp, and interact with the user

with subprocess.Popen([wrapperExecutable, fgFileName], \
                      stdin=subprocess.PIPE, \
                      stdout=subprocess.PIPE, \
                      universal_newlines=True) as wrapperProc:

    def execWrapperCmd(fwdCmd):
        logging.info('Driver to wrapper: ' + fwdCmd)
        print(fwdCmd, file=wrapperProc.stdin)
        wrapperProc.stdin.flush()
        response = wrapperProc.stdout.readline().strip()
        logging.info('Wrapper to driver: ' + response)
        return response

    def observe(t, value):
        fwdCmd = 'O {0} {1}'.format(bnetDict[t], 'true' if value else 'false')
        execWrapperCmd(fwdCmd)

    def getRankedAlarms():
        alarmList = []
        for t in baseQueries:
            index = bnetDict[t]
            response = float(execWrapperCmd('Q {0}'.format(index)))
            alarmList.append((t, response))
        return sorted(alarmList, key=lambda rec: (-rec[1], rec[0]))

    def printRankedAlarms(outFile):
        alarmList = getRankedAlarms()
        print('Rank\tConfidence\tTuple', file=outFile)
        index = 0
        for t, confidence in alarmList:
            index = index + 1
            print('{0}\t{1}\t{2}'.format(index, confidence, t), file=outFile)


    def runAlarmRanking(tolerance, minIters, maxIters, histLength, outFile):
        yetToConvergeFraction = float(execWrapperCmd('BP {0} {1} {2} {3}'.format(tolerance, minIters, maxIters, histLength)))
        rankedAlarmList = getRankedAlarms()
        printRankedAlarms(outFile)


    logging.info('Awaiting command')
    for command in sys.stdin:
        command = command.strip()
        logging.info('Read command {0}'.format(command))

        components = [ c.strip() for c in re.split(' |\t', command) if len(c.strip()) > 0 ]
        if len(components) == 0: continue

        cmdType = components[0]
        components = components[1:]

        if cmdType == 'Q':
            # 2a. Marginal probability query.
            # Syntax: Q t.
            # Output: t belief(t).
            t = components[0]
            fwdCmd = 'Q {0}'.format(bnetDict[t])
            print('{0} {1}'.format(t, float(execWrapperCmd(fwdCmd))))

        elif cmdType == 'FQ':
            # 2b. Factor marginal.
            # Syntax: FQ f i.
            # Output: belief(f, i).
            # Note: No encoding or decoding is performed for this command. It is intended to be used by em.py, which can
            # do these things on its own.
            print(float(execWrapperCmd(command)))

        elif cmdType == 'BP':
            # 2c. Run belief propagation.
            # Syntax: BP tolerance minIters maxIters histLength.
            # Output: 'converged' if belief propagation converged, or 'diverged' otherwise.
            tolerance = float(components[0])
            minIters = int(components[1])
            maxIters = int(components[2])
            histLength = int(components[3])
            assert 0 < tolerance and tolerance < 1
            assert 0 < histLength and histLength < minIters and minIters < maxIters
            print(execWrapperCmd('BP {0} {1} {2} {3}'.format(tolerance, minIters, maxIters, histLength)))

        elif cmdType == 'O':
            # 2e. Observe oracle data.
            # Syntax: O t value.
            # Output: 'O t value'. Merely an acknowledgment that the command was received.
            t = components[0]
            assert components[1] == 'true' or components[1] == 'false'
            value = (components[1] == 'true')
            observe(t, value)
            print('O {0} {1}'.format(t, 'true' if value else 'false'))

        elif cmdType == 'P':
            # 2f. Printing ranked list of alarms to file
            # Syntax: P filename.
            # Output: Ranked list of alarms, in the format of combined.out. Printed to filename. Acknowledgment printed
            # to stdout.
            outFileName = components[0]
            with open(outFileName, 'w') as outFile: printRankedAlarms(outFile)
            print('P {0}'.format(outFileName))

        elif cmdType == 'HA':
           # 2g. Get the alarm with the highest ranking and maximum confidence.
           # Syntax: HA.
           # Output: A tuple t
           alarmList = getRankedAlarms()
           topAlarm, confidence = alarmList[0]
           print('{0} {1}'.format(topAlarm, confidence))

        elif cmdType == 'AR':
            tolerance = float(components[0])
            minIters = int(components[1])
            maxIters = int(components[2])
            histLength = int(components[3])
            outFileName = components[4]
            assert 0 < tolerance and tolerance < 1
            assert 0 < histLength and histLength < minIters and minIters < maxIters
            with open(outFileName, 'w') as outFile:
                runAlarmRanking(tolerance, minIters, maxIters, histLength, outFile)

        else:
            assert cmdType == 'NL', 'Unexpected command {0}!'.format(command)
            print()

        sys.stdout.flush()
        logging.info('Awaiting command')

logging.info('Bye!')
