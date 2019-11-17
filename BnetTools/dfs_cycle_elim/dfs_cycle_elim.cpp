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


/******** Global Variables ****************/

unordered_set<size_t> deletedCids;

/******** End of Global Variables *********/


/******** File Static Variables ****************/

static unordered_set<size_t> visited;
static unordered_set<size_t> dfsFinished;

/******** End of File Static Variables *********/

static size_t maxAntecedentDob(size_t cid, unordered_map<size_t, size_t>& tid2Dob) {
    size_t ans = 0;
    for (size_t antId : cid2AntecedentTids[cid]) {
        ans = max(ans, tid2Dob.at(antId));
    }
    return ans;
}


static void genConstraints (unordered_set<size_t>& cyclicCids) {
    for (const auto& clause : *workClauses) {
        size_t cid = clause2Cid.at(clause);
        if (cyclicCids.find(cid) == cyclicCids.end()) {
            vector<string> cl = cid2Clause[cid];
            resultClauses.insert(cl);
        }
    }
    return;
}


static bool checkAcyclicity (size_t t, unordered_map<size_t, set<size_t>>& neighbours) {
    size_t numBackEdges = 0;
    stack<size_t> dfsStack;
    dfsFinished.clear();
    visited.clear();
    dfsStack.push(t);
    while (!dfsStack.empty()) {
        size_t topElem = dfsStack.top();
        if (visited.find(topElem) == visited.end()) {
            visited.insert(topElem);
            for (const auto adjElem : neighbours.at(topElem)) {
                if (visited.find(adjElem) == visited.end()) {
                    dfsStack.push(adjElem);
                } else if (dfsFinished.find(adjElem) == dfsFinished.end()) {
                    numBackEdges++;
                } 
            }
        } else {
            dfsStack.pop();
            dfsFinished.insert(topElem);
        }
        if (numBackEdges > 0) break;
    }
    if (numBackEdges == 0)
        return true;
    else
        return false;
}


unordered_map<size_t, size_t> getDob () {

    unordered_map<size_t, size_t> tid2Dob;
    for (const auto& tup : *workTuples) {
        size_t t = tuple2Tid.at(tup);
        if (workInputTids->find(t) == workInputTids->end() &&
            allInputTids.find(t) == allInputTids.end())
            tid2Dob[t] = numeric_limits<size_t>::max();
        else
            tid2Dob[t] = 0;
    }

    size_t numChanged = 1;
    while (numChanged > 0) {
        numChanged = 0;
        for (const auto& clause : *workClauses) {
            size_t cid = clause2Cid.at(clause);
            auto consequentTid = cid2ConsequentTid.at(cid);
            size_t newDob = maxAntecedentDob(cid, tid2Dob);
            if (newDob < numeric_limits<size_t>::max()) { newDob++; }
            if (newDob < tid2Dob[consequentTid]) {
                numChanged++;
                tid2Dob[consequentTid] = newDob;
            }
        }
    }

    size_t maxDob = 0, unreachableTuples = 0;
    for (const auto& tidDobPair : tid2Dob) {
        maxDob = max(maxDob, tidDobPair.second);
        if (tidDobPair.second == numeric_limits<size_t>::max()) {
            unreachableTuples++;
        }
    }
    clog << "eliminateCycles: unreachable tuples: " << unreachableTuples << endl;
    return tid2Dob;
}


void eliminateCycles () {
    set<size_t> unprocessedCids;
    unordered_set<size_t> acyclicCids;
    unordered_set<size_t> cyclicCids;
    unordered_map<size_t, set<size_t>> neighbours;

    deletedCids.clear();
    unordered_map<size_t, size_t> tid2Dob = getDob();
    for (const auto t : *workConsequentTids) {
        neighbours[t] = set<size_t>();
    }
    for (vector<string> clause : *workClauses) {
        size_t cid = clause2Cid.at(clause);
        auto consequentTid = cid2ConsequentTid[cid];
        size_t maxAntDob = maxAntecedentDob(cid, tid2Dob);
        bool fwdClause = (maxAntDob < tid2Dob.at(consequentTid));
        if (fwdClause) {
            acyclicCids.insert(cid);
            for (const auto antTid : cid2AntecedentTids[cid]) {
                neighbours[antTid].insert(consequentTid);
            }
        } else
            unprocessedCids.insert(cid);
    }
 
    clog << __LOGSTR_FULL__ << "eliminateCycles: number of bkwd clauses: " << to_string(unprocessedCids.size()) << endl;
    size_t numProcessed = 0;
    for (size_t uCid : unprocessedCids) {
        unordered_set<size_t> modifiedAntTids;
        size_t consequentTid = cid2ConsequentTid[uCid];
        for (const auto antTid : cid2AntecedentTids[uCid]) {
            if (neighbours[antTid].find(consequentTid) == neighbours[antTid].end()) {
                neighbours[antTid].insert(consequentTid);
                modifiedAntTids.insert(antTid);
            }
        }
        bool isAcyclic = checkAcyclicity(consequentTid, neighbours);
        if (!isAcyclic) {
            cyclicCids.insert(uCid);
            for (const auto antTid : modifiedAntTids) {
                neighbours[antTid].erase(consequentTid);
            }
        } 
        numProcessed++;
        if (numProcessed % 1000 == 0) 
            clog << __LOGSTR_FULL__ << "eliminateCycles: completed: " << to_string(numProcessed) << endl;
    }
    for (size_t cid : cyclicCids) deletedCids.insert(cid);
    genConstraints(cyclicCids);
    clog << __LOGSTR_FULL__ << "eliminateCycles: Deleted " << to_string(cyclicCids.size())  << " grounded constraints." << endl;
    return;
}
