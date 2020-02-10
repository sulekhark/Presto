#!/usr/bin/env python3

# ./preprocess_doms.py 
#
# ASSUMPTION: This script executes in the reports/metadata dir of the benchmark.

import logging
import re
import sys

srcRootFileName = "../../datalog/source_root.txt"
daffodilPrefix = "Daffodil.Stubs"

TFileName = "../../datalog/T.map"
FFileName = "../../datalog/F.map"
IFileName = "../../datalog/I.map"
HFileName = "../../datalog/H.map"
PFileName = "../../datalog/P.map"
EHFileName = "../../datalog/EH.map"
MFileName = "../../datalog/M.map"
throwPVFileName = "../../datalog/ThrowPV.txt"
varEHFileName = "../../datalog/VarEH.txt"

TOut = "T.out"
FOut = "F.out"
IOut = "I.out"
HOut = "H.out"
POut = "P.out"
EHOut = "EH.out"
MOut = "M.out"
VOut = "V.out"

logging.basicConfig(level=logging.INFO, \
                            format="[%(asctime)s] %(levelname)s [%(name)s.%(funcName)s:%(lineno)d] %(message)s", \
                                                datefmt="%H:%M:%S")


########################################################################################################################
# Read in the various map files 


srcRoot = [ line.strip() for line in open(srcRootFileName) ]
assert len(srcRoot) == 1
srcRootStr = srcRoot[0] + "\\"


TFile = open(TOut, 'w')
TEntries = [ line.strip() for line in open(TFileName) ]
cnt = 0
for entry in TEntries:
    txt = "TYPE:" + entry
    print("{0}:{1}".format(cnt, txt), file=TFile)
    cnt = cnt + 1
TFile.close()


FFile = open(FOut, 'w')
FEntries = [ line.strip() for line in open(FFileName) ]
cnt = 0
for entry in FEntries:
    entry = entry.replace("  CLASS:", " @")
    txt = "FIELD:" + entry
    print("{0}:{1}".format(cnt, txt), file=FFile)
    cnt = cnt + 1
FFile.close()


MFile = open(MOut, 'w')
MEntries = [ line.strip() for line in open(MFileName) ]
cnt = 0
for entry in MEntries:
    cpos = entry.find("  CLASS:")
    if cpos > -1:
        entry = entry[:cpos]
    txt = "METHOD:" + entry
    print("{0}:{1}".format(cnt, txt), file=MFile)
    cnt = cnt + 1
MFile.close()


IFile = open(IOut, 'w')
IEntries = [ line.strip() for line in open(IFileName) ]
cnt = 0
for entry in IEntries:
    pos1 = entry.find("SRCFILE:")
    if "SRCFILE:NA" in entry:
        s = entry[:pos1]
        pos2 = s.find("METHOD:")
        t = s.split("CLASS:")[0] + s[pos2:]
    else:
        t = entry[pos1:]
        dafPos = t.find(daffodilPrefix)
        if dafPos > -1:
            t = t[dafPos:]
        else:
            t = t.replace(srcRootStr, "")
            t = t.replace("SRCFILE:", "")
        t = t.replace("SRCLINE", "")
    txt = "INVK AT:" + t 
    print("{0}:{1}".format(cnt, txt), file=IFile)
    cnt = cnt + 1
IFile.close()


HFile = open(HOut, 'w')
HEntries = [ line.strip() for line in open(HFileName) ]
cnt = 0
for entry in HEntries:
    pos1 = entry.find("SRCFILE:")
    if "SRCFILE:NA" in entry:
        s = entry[:pos1]
        pos2 = s.find("METHOD:")
        t = s.split("CLASS:")[0] + s[pos2:]
    else:
        t = entry[pos1:]
        dafPos = t.find(daffodilPrefix)
        if dafPos > -1:
            t = t[dafPos:]
        else:
            t = t.replace(srcRootStr, "")
            t = t.replace("SRCFILE:", "")
        t = t.replace("SRCLINE", "")
    txt = "ALLOC AT:" + t 
    print("{0}:{1}".format(cnt, txt), file=HFile)
    cnt = cnt + 1
HFile.close()


PMap = {}
PFile = open(POut, 'w')
PEntries = [ line.strip() for line in open(PFileName) ]
cnt = 0
for entry in PEntries:
    pos1 = entry.find("SRCFILE:")
    if "SRCFILE:NA" in entry:
        s = entry[:pos1]
        pos2 = s.find("METHOD:")
        t = s.split("CLASS:")[0] + s[pos2:]
    else:
        t = entry[pos1:]
        dafPos = t.find(daffodilPrefix)
        if dafPos > -1:
            t = t[dafPos:]
        else:
            t = t.replace(srcRootStr, "")
            t = t.replace("SRCFILE:", "")
        t = t.replace("SRCLINE", "")
    txt = "PGM POINT:" + t 
    print("{0}:{1}".format(cnt, txt), file=PFile)
    PMap[cnt] = txt
    cnt = cnt + 1
PFile.close()


EHMap = {}
EHFile = open(EHOut, 'w')
EHEntries = [ line.strip() for line in open(EHFileName) ]
cnt = 0
for entry in EHEntries:
    pos1 = entry.find("SRCFILE:")
    if "SRCFILE:NA" in entry:
        s = entry[:pos1]
        pos2 = s.find("METHOD:")
        t = s.split("CLASS:")[0] + s[pos2:]
    else:
        t = entry[pos1:]
        dafPos = t.find(daffodilPrefix)
        if dafPos > -1:
            t = t[dafPos:]
        else:
            t = t.replace(srcRootStr, "")
            t = t.replace("SRCFILE:", "")
        t = t.replace("SRCLINE", "")
    txt = "CATCH BLK:" + t 
    print("{0}:{1}".format(cnt, txt), file=EHFile)
    EHMap[cnt] = txt
    cnt = cnt + 1
EHFile.close()


VFile = open(VOut, 'w')
throwPVEntries = [ line.strip() for line in open(throwPVFileName) ]
for entry in throwPVEntries:
    parts = entry.split(",   ")
    pNdx = parts[1].split(':',1)[0]
    vNdx = parts[2].split(':',1)[0]
    txt = "VAR AT " + PMap[int(pNdx)]
    print("{0}:{1}".format(vNdx, txt), file=VFile)

varEHEntries = [ line.strip() for line in open(varEHFileName) ]
for entry in varEHEntries:
    entry = entry[6:] # remove VarEH(
    parts = entry.split(",   ")
    ehNdx = parts[0].split(':',1)[0]
    vNdx = parts[1].split(':',1)[0]
    txt = "VAR OF " + EHMap[int(ehNdx)]
    print("{0}:{1}".format(vNdx, txt), file=VFile)
VFile.close()
