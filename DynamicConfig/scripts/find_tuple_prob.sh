#!/bin/bash

# executes in bnet directory

tupleFile=$1
bnetDictFile=$2
outputFile=$3

dynLogDir=../dynlogs
dynCfgDir=../dynconfig
datalogDir=../datalog

pnMap=$datalogDir/PNMap.datalog
methodMap=$dynCfgDir/id_to_method_map.txt
excMap=$dynCfgDir/id_to_exctype_map.txt

minProb=0.1
maxProb=0.95

tempFile=temp_file.txt
rm -rf $outputFile

#####################################################################
function getLoc
{
    local pp=$1
    local str=`grep "PNMap($pp," $pnMap`
    ppLoc="$(echo $str | cut -d',' -f2 | cut -d ')' -f1)"
}

function getMethodName
{
    local methId=$1
    local str=`grep "$methId:" $methodMap`
    methName="$(echo $str | cut -d':' -f2 | cut -d '(' -f1)"
}


function getExcType
{
    local excId=$1
    local str=`grep "$excId:" $excMap`
    excType="$(echo $str | cut -d':' -f2)"
}

#####################################################################

while read -r tupl
do
    bnetNodeId=`grep -v NOT $bnetDictFile | grep $tupl | cut -d':' -f1`
    if [[ $tupl == EscapeMTP* ]]
    then
        tuplArgs="$(echo $tupl | cut -d'(' -f2 | cut -d ')' -f1)"
        methId="$(echo $tuplArgs | cut -d',' -f1)"
        excTypeId="$(echo $tuplArgs | cut -d',' -f2)"
        ppId="$(echo $tuplArgs | cut -d',' -f3)"
        getLoc $ppId
        getMethodName $methId
        getExcType $excTypeId

        dirName="$dynLogDir/FaultInjectionSet/EscapeMTP/T_${methId}_${ppLoc}_*_${excTypeId}"
        if [[ $methName == *.Main ]]
        then
            mCnt=0
            mExcCnt=0
            for logfile in `find $dirName -name execution_log`
            do
                mCnt=$((mCnt + 1)) 
                grep -q $excType $logfile 
                if [ $? -eq 0 ]; then
                    mExcCnt=$((mExcCnt + 1))
                fi
            done 
            if [ $mCnt -eq 0 ]
            then
                prob=$minProb
            else
                prob=`echo "scale=2; $minProb + (($maxProb - $minProb) * $mExcCnt / $mCnt)" | bc`
            fi
            # echo $tupl $prob $mCnt $mExcCnt
            echo "$bnetNodeId:" "$prob" >> $outputFile
        else
            mCnt_tot=0
            mExcCnt_tot=0
            for logfile in `find $dirName -name torch*`
            do
                awk -F';' '{print $8 $15;}' $logfile > $tempFile
                mCnt=`grep $methName $tempFile | wc -l`
                mExcCnt=`grep $methName $tempFile | grep $excType | wc -l`
                mCnt_tot=$((mCnt_tot + mCnt))
                mExcCnt_tot=$((mExcCnt_tot + mExcCnt))
            done 
            if [ $mCnt_tot -eq 0 ]
            then
                prob=$minProb
            else
                prob=`echo "scale=2; $minProb + (($maxProb - $minProb) * $mExcCnt_tot / $mCnt_tot)" | bc`
            fi
            # echo $tupl $prob $mCnt_tot $mExcCnt_tot
            echo "$bnetNodeId:" "$prob" >> $outputFile
        fi
    fi
    if [[ $tupl == CallAt* ]]
    then
        tuplArgs="$(echo $tupl | cut -d'(' -f2 | cut -d ')' -f1)"
        callerId="$(echo $tuplArgs | cut -d',' -f1)"
        ppId="$(echo $tuplArgs | cut -d',' -f2)"
        calleeId="$(echo $tuplArgs | cut -d',' -f3)"
        getLoc $ppId
        getMethodName $callerId
        callerName=$methName
        getMethodName $calleeId
        calleeName=$methName

        dirName="$dynLogDir/Logging"
        mCnt_tot=0
        mCalleeCnt_tot=0
        for logfile in `find $dirName -name torch*`
        do
            mCnt=`awk -F';' -v callerN="$callerName" '$7 ~ callerN' $logfile | awk -F';' -v callP="$ppLoc" '$11 == callP' | wc -l`
            mCalleeCnt=`awk -F';' -v callerN="$callerName" '$7 ~ callerN' $logfile | awk -F';' -v callP="$ppLoc" '$11 == callP' | awk -F';' -v calleeN="$calleeName" '$8 ~ calleeN' | wc -l`
            mCnt_tot=$((mCnt_tot + mCnt))
            mCalleeCnt_tot=$((mCalleeCnt_tot + mCalleeCnt))
        done 
        if [ $mCnt_tot -eq 0 ]
        then
            prob=$minProb
        else 
            prob=`echo "scale=2; $minProb + (($maxProb - $minProb) * $mCalleeCnt_tot / $mCnt_tot)" | bc`
        fi
        # echo $tupl $prob $mCnt_tot $mCalleeCnt_tot
        echo "$bnetNodeId:" "$prob" >> $outputFile
    fi
    if [[ $tupl == CtxtCallAt* ]]
    then
        tuplArgs="$(echo $tupl | cut -d'(' -f2 | cut -d ')' -f1)"
        ctxtCallerId="$(echo $tuplArgs | cut -d',' -f1)"
        ctxtPpId="$(echo $tuplArgs | cut -d',' -f2)"
        callerId="$(echo $tuplArgs | cut -d',' -f3)"
        ppId="$(echo $tuplArgs | cut -d',' -f4)"
        calleeId="$(echo $tuplArgs | cut -d',' -f5)"

        getMethodName $ctxtCallerId
        ctxtCallerName=$methName
        getLoc $ctxtPpId
        ctxtPpLoc=$ppLoc
        getMethodName $callerId
        callerName=$methName
        getLoc $ppId
        getMethodName $calleeId
        calleeName=$methName

        dirName="$dynLogDir/Logging"
        mCnt_tot=0
        mCalleeCnt_tot=0
        for logfile in `find $dirName -name torch*`
        do
            if [ $ctxtCallerName == $callerName -a $ctxtPpLoc == $ppLoc ]
            then
                mCnt=`awk -F';' -v callerN="$callerName" '$7 ~ callerN' $logfile | awk -F';' -v callP="$ppLoc" '$11 == callP' | wc -l`
                mCalleeCnt=`awk -F';' -v callerN="$callerName" '$7 ~ callerN' $logfile | awk -F';' -v callP="$ppLoc" '$11 == callP' | awk -F';' -v calleeN="$calleeName" '$8 ~ calleeN' | wc -l`
                mCnt_tot=$((mCnt_tot + mCnt))
                mCalleeCnt_tot=$((mCalleeCnt_tot + mCalleeCnt))
            else
                ctxtSeqNum=`awk -F';' -v ctcallerN="$ctxtCallerName" '$7 ~ ctcallerN' $logfile | awk -F';' -v ctcallP="$ctxtPpLoc" '$11 == ctcallP' | awk -F';' -v calleeN="$callerName" '$8 ~ calleeN' | awk -F';' '{print $4;}' | head -n 1`
                mCnt=`awk -F';' -v callerN="$callerName" '$7 ~ callerN' $logfile | awk -F';' -v callP="$ppLoc" '$11 == callP' | awk -F';' -v ctN="$ctxtSeqNum" '$4 > ctN+0' | wc -l`
                mCalleeCnt=`awk -F';' -v callerN="$callerName" '$7 ~ callerN' $logfile | awk -F';' -v callP="$ppLoc" '$11 == callP' | awk -F';' -v calleeN="$calleeName" '$8 ~ calleeN' | awk -F';' -v ctN="$ctxtSeqNum" '$4 > ctN+0' | wc -l`
                mCnt_tot=$((mCnt_tot + mCnt))
                mCalleeCnt_tot=$((mCalleeCnt_tot + mCalleeCnt))
            fi
        done 
        if [ $mCnt_tot -eq 0 ]
        then
            prob=$minProb
        else
            prob=`echo "scale=2; $minProb + (($maxProb - $minProb) * $mCalleeCnt_tot / $mCnt_tot)" | bc`
        fi
        # echo $tupl $prob $mCnt_tot $mCalleeCnt_tot
        echo "$bnetNodeId:" "$prob" >> $outputFile
    fi
done < $tupleFile
rm -f $tempFile
