#include <boost/algorithm/string/predicate.hpp>
using boost::starts_with;

#include <algorithm>
#include <cassert>
#include <chrono>
#include <ctime>
#include <iostream>
#include <iterator>
#include <limits>
#include <map>
#include <set>
#include <string>
#include <utility>
#include <unordered_map>
#include <unordered_set>
#include <vector>
#include <stack>
#include <fstream>
using namespace std;

#include "util.h"
#include "framework.h"
#include "main.h"


void getCoreachable (set<string>& allOutputTuples) {

    resultClauses.clear();
    unordered_set<size_t> allOutputTids;
    for (string tup : allOutputTuples) allOutputTids.insert(tuple2Tid.at(tup));
    for (const auto& t : allOutputTids) {
        assert(workConsequentTids->find(t) != workConsequentTids->end());
    }
    unordered_set<size_t> coreachableTids = allOutputTids;
    unordered_set<size_t> unprocessedTids = coreachableTids;
    while (!unprocessedTids.empty()) {
        const auto t = setPop(&unprocessedTids);
        for (const auto& cid : consequentTid2Cids.at(t)) {
            if (workCids->find(cid) != workCids->end()) {
                assert(t == cid2ConsequentTid[cid]);
                for (const auto& tPrime : cid2AntecedentTids[cid]) {
                    if (coreachableTids.find(tPrime) == coreachableTids.end()) {
                        if (workInputTids->find(tPrime) == workInputTids->end() &&
                            allInputTids.find(tPrime) == allInputTids.end()) {
                            unprocessedTids.insert(tPrime);
                        }
                    }
                }
            }
        }
        coreachableTids.insert(t);
    }

    // Compute the set of active clauses
    for (const auto& clause : *workClauses) {
        const auto cid = clause2Cid.at(clause);
        const auto consequentTid = cid2ConsequentTid.at(cid);
        if (coreachableTids.find(consequentTid) != coreachableTids.end()) {
            resultClauses.insert(clause);
        }
    }
    clog << "getCoreachable: Discovered " << coreachableTids.size() << " coreachable tuples, "
         << resultClauses.size() << " active clauses." << endl;
    return;
}


void getLevelBasedCoreachable(set<string>& allOutputTuples, int numLevels) {

    resultClauses.clear();
    unordered_set<size_t> allOutputTids;
    for (string tup : allOutputTuples) allOutputTids.insert(tuple2Tid.at(tup));
    for (const auto& t : allOutputTids) {
        assert(workConsequentTids->find(t) != workConsequentTids->end());
    }
    unordered_set<size_t> coreachableTids = allOutputTids;
    unordered_set<size_t> unprocessedTids = coreachableTids;
    unordered_set<size_t> newUnprocessedTids;
    int lvlCnt = 0;
    while (numLevels > 0) {
        while (!unprocessedTids.empty()) {
            const auto t = setPop(&unprocessedTids);
            for (const auto& cid : consequentTid2Cids.at(t)) {
                if (workCids->find(cid) != workCids->end()) {
                    assert(t == cid2ConsequentTid[cid]);
                    for (const auto& tPrime : cid2AntecedentTids[cid]) {
                        if (coreachableTids.find(tPrime) == coreachableTids.end()) {
                            if (workInputTids->find(tPrime) == workInputTids->end() &&
                                allInputTids.find(tPrime) == allInputTids.end()) {
                                newUnprocessedTids.insert(tPrime);
                            }
                        }
                    }
                }
            }
            coreachableTids.insert(t);
        }
        lvlCnt++;
        numLevels --;
        clog << "Coreachable tuples at level: " << lvlCnt << " are: " << endl;
        int actualCnt = 0;
        for (size_t tid : newUnprocessedTids) {
            if (coreachableTids.find(tid) == coreachableTids.end()) {
                unprocessedTids.insert(tid);
                clog << tid2Tuple.at(tid) << "  ";
                actualCnt++;
            }
        }
        clog << endl << "Total coreachable at level: " << lvlCnt << " is: " << actualCnt << endl;
    }
    // Compute the set of active clauses
    for (const auto& clause : *workClauses) {
        const auto cid = clause2Cid.at(clause);
        const auto consequentTid = cid2ConsequentTid.at(cid);
        if (coreachableTids.find(consequentTid) != coreachableTids.end()) {
            resultClauses.insert(clause);
        }
    }
    clog << "getCoreachable: Discovered " << coreachableTids.size() << " coreachable tuples, "
         << resultClauses.size() << " active clauses." << endl;
    return;
}


// The specified set of input tuples are coreachable from each of the specified output tuples.
bool coreachSanityCheck (set<string>& allOutputTuples, set<string>& scTuples) {
    bool sanityCheckPassed = true;
    set<string> sanityFailedTuples;
    for (string opTup : allOutputTuples) {
        set<string> checkSet;
        checkSet.insert(opTup);
        getCoreachable(checkSet);
        unordered_set<string> resConsequents;
        for (const auto& clause : resultClauses) {
            string resConsequent = getConsequent(clause);
            resConsequents.insert(resConsequent);
        }
        bool found = true;
        for (string scTup : scTuples) {
            if (resConsequents.find(scTup) == resConsequents.end()) {
                found = false;
                break;
            } 
        }
        if (!found) {
            sanityCheckPassed = false;
            sanityFailedTuples.insert(opTup); 
        }
        clog << "Tuple: " << opTup << " ipTuple coreachable: " << (found ? "true" : "false") << endl;
    }
    clog << "Coreachability-based sanity check passed: " << (sanityCheckPassed ? "true" : "false") << endl;
    if (!sanityCheckPassed) {
        printSet("Coreachability-based sanity check failed for the following output tuples", sanityFailedTuples);
    }
    return sanityCheckPassed;
}
