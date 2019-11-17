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
#include <list>
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

/*********************************
This file contains methods that are specific to the analysis.
Therefore, they make assumptions that depend on the syntax and structure of the analysis rules.
*********************************/


// Following assumptions are made:
// 1. The set methTempl contains one template. Ex.: PathEdge_cs(0,22324,
// 2. If the template matches a consequent of a clause, it represents a method invoke.
//    We first get all clauses where the consequent matches the template, and then extract the consequent.
// 3. Multiple clauses might have the same consequent. This happens when different invk statements call the
//    same method in the same state.
// 4. Each state in which a method is to be analyzed is treated as a separate "method" for performing
//    inference and generating a summary.
set<set<string>> getMethodFrontiers(set<string>& methTempl, set<vector<string>>& currentClauses) {
    set<set<string>> methFrontiersSet;
    set<string> matchingConsequents;
    string templ;
    // ASSUMPTION: There is only one template in methTempl
    templ = (string)*(methTempl.begin());
    if (templ == "PathEdge_cs(0,0,") templ = "PathEdge_cs(0,1,"; // For main method, elide-edb removes "PathEdge_cs(0,0,1,0,0)"
    for (vector<string> clause : currentClauses) {
        string consequent = getConsequent(clause);
        if (starts_with(consequent, templ)) matchingConsequents.insert(consequent); 
    }
    for (string con : matchingConsequents) {
        set<string> conSet;
        conSet.insert(con);
        methFrontiersSet.insert(conSet);
    }
    return methFrontiersSet;
}


// Following assumptions are made:
// 1. The set methTempl contains one template. Ex.: PathEdge_cs(0,22324,
// 2. We are searching for method frontier tuples that are consumed by the clauses representing the method body.
set<string> getConsumedMethodFrontiers(set<string>& methTempl, set<vector<string>>& currentClauses) {
    set<string> methFrontiers;
    unordered_set<string> methConsequents;
    string templ;
    // ASSUMPTION: There is only one template in methTempl
    templ = (string)*(methTempl.begin());
    if (templ == "PathEdge_cs(0,0,") templ = "PathEdge_cs(0,1,"; // For main method, elide-edb removes "PathEdge_cs(0,0,1,0,0)"
    for (vector<string> clause : currentClauses) {
        string consequent = getConsequent(clause);
        methConsequents.insert(consequent);
    }
    for (vector<string> clause : currentClauses) {
        vector<string> antecedents = getAntecedents(clause);
        for (string ant : antecedents) {
            if (starts_with(ant, templ) && methConsequents.find(ant) == methConsequents.end())
                methFrontiers.insert(ant); 
        }
    }
    return methFrontiers;
}


// Following assumptions are made:
// 1. The observables are "mhe_cs" tuples and the "PathEdge_cs" tuples corresponding to the return statement.
//    We first get all clauses where the consequent matches any template, and then extract the consequent.
// 2. Multiple clauses might have the same consequent. This happens when the successor of different statements 
//    in the  method is the return statement.
set<string> getMethodObservables(set<string>& methTempl, set<vector<string>>& currentClauses) {
    set<string> methObservables;
    for (vector<string> clause : currentClauses) {
        string consequent = getConsequent(clause);
        for (string templ : methTempl)
            if (starts_with(consequent, templ)) methObservables.insert(consequent); 
    }
    return methObservables;
}


set<string> getMethodClObservables(set<string>& methTempl, set<vector<string>>& currentClauses) {
    set<string> methObservables;
    string templ = (string)(*(methTempl.begin()));
    for (vector<string> clause : currentClauses) {
        string consequent = getConsequent(clause);
        if (starts_with(consequent, templ)) methObservables.insert(consequent);
    }
    return methObservables;
}


set<string> getMethodMemObservables(set<string>& methTempl, set<vector<string>>& currentClauses) {
    set<string> methObservables;
    string templ = (string)(*(++methTempl.begin()));
    for (vector<string> clause : currentClauses) {
        string consequent = getConsequent(clause);
        if (starts_with(consequent, templ)) methObservables.insert(consequent);
    }
    return methObservables;
}


bool tupleMatchesObservable(set<string>& methTempl, string tup) {
    for (string templ : methTempl)
        if (starts_with(tup, templ)) return true;
    return false; 
}


set<string> getObservableAntecedents(set<string>& methTempl, vector<string>& clause) {
    set<string> obsAntecedents;
    vector<string> antVec = getAntecedents(clause);
    for (string ant : antVec) {
        for (string templ : methTempl)
            if (starts_with(ant, templ)) obsAntecedents.insert(ant); 
    }
    return obsAntecedents;
}


set<string> getNonObservableAntecedents(set<string>& methTempl, vector<string>& clause) {
    set<string> nonObsAnts;
    vector<string> antVec = getAntecedents(clause);
    for (string ant : antVec) {
        bool isNonObs = true;
        for (string templ : methTempl)
            if (starts_with(ant, templ)) isNonObs = false;
        if (isNonObs) nonObsAnts.insert(ant); 
    }
    return nonObsAnts;
}


set<string> getSanityCheckTuples() {
    set<string> scTuples;
    scTuples.insert("PathEdge_cs(0,1,1,0,0)");
    return scTuples;
}


string getMainMethod() {
    return "CM(0,0)";
}


set<string> getNonSummarizableMethods() {
    set<string> nsMethods;
    nsMethods.insert("CM(0,1)");
    return nsMethods;
}


set<string> getClusterTupleTemplates() {
    set<string> clusTempls;
    clusTempls.insert("escO");
    return clusTempls;
}


string getCecId(string tup) {
    unsigned first = tup.find(',');
    string tup1 = tup.substr(first + 1);
    unsigned last = tup1.find(',');
    string cecId = tup1.substr(0,last);
    return cecId;
}


bool isCecTuple(string tup) {
    if (starts_with(tup, "CEC")) return true;
    return false;
}

bool isEscapingRaceTuple(string tup) {
    if (starts_with(tup, "escapingRaceHext")) return true;
    return false;
}
