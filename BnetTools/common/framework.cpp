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

/******** Global Variables *********/

set<string> allTuples;
set<vector<string>> allClauses;
unordered_set<size_t> allInputTids;
set<size_t> allConsequentTids;
unordered_set<size_t> allTids;
unordered_set<size_t> allCids;

set<string> methTuples;
set<vector<string>> methClauses;
unordered_set<size_t> methInputTids;
set<size_t> methConsequentTids;
unordered_set<size_t> methTids;
unordered_set<size_t> methCids;

set<string> *workTuples;
set<vector<string>> *workClauses;
unordered_set<size_t> *workInputTids;
set<size_t> *workConsequentTids;
unordered_set<size_t> *workTids;
unordered_set<size_t> *workCids;

// unordered_map<vector<string>, size_t, vector_hash<string>> clause2Cid;
map<vector<string>, size_t> clause2Cid;
unordered_map<size_t, vector<string>> cid2Clause;
unordered_map<string, size_t> tuple2Tid;
unordered_map<size_t, string> tid2Tuple;
unordered_map<size_t, double> cid2Prob;
unordered_map<size_t, string> cid2RuleName;
unordered_map<string, unordered_set<size_t>> ruleName2Cids;
unordered_map<size_t,size_t> cid2ConsequentTid;
unordered_map<size_t, set<size_t>> cid2AntecedentTids;
unordered_map<size_t, set<size_t>> consequentTid2Cids;
unordered_map<size_t, set<size_t>> antecedentTid2Cids;

set<vector<string>> resultClauses;

double defaultRuleProb = 0.999;
size_t generatedNameNdx;
size_t intermediateSaveFrequency;
static size_t cid = 0;  // Clause Id starts from 0

/******** End of Global Variables *********/


string lit2Tuple(const string& literal) {
    if (starts_with(literal, "NOT ")) { return literal.substr(4); }
    else { return literal; }
}


vector<string> getAntecedents(const vector<string>& clause) {
    if (clause.size() > 1) {
        vector<string> ans(&clause[0], &clause[clause.size() - 1]);
        assert(clause.size() == ans.size() + 1);
        transform(ans.begin(), ans.end(), ans.begin(), &lit2Tuple);
        return ans;
    } else { return vector<string>(); }
}


string getConsequent(const vector<string>& clause) {
    string ans = clause[clause.size() - 1];
    assert(!starts_with(ans, "NOT "));
    return ans;
}


void printSet (string str, set<size_t> &s) {
    clog << __LOGSTR__ << "printSet: " << str <<  ": ";
    for (auto v : s) clog << to_string(v) << " ";
    clog << endl;
    return;
}


void printUSet (string str, unordered_set<size_t> &s) {
    clog << __LOGSTR__ << "printUSet: " << str <<  ": ";
    for (auto v : s) clog << to_string(v) << " ";
    clog << endl;
    return;
}


void printSet (string str, set<string> &s) {
    clog << __LOGSTR__ << "printSet: " << str <<  ": ";
    for (auto v : s) clog << v << " ";
    clog << endl;
    return;
}


bool isSelfLoop(const vector<string>& clause) {
    string consequent = getConsequent(clause);
    for (const auto& ant : getAntecedents(clause)) {
        if (lit2Tuple(ant) == consequent) return true;
    }
    return false;
}


void setWorkingAll() {
    workTuples = &allTuples;
    workClauses = &allClauses;
    workInputTids = &allInputTids;
    workConsequentTids = &allConsequentTids;
    workTids = &allTids;
    workCids = &allCids;
    resultClauses.clear();
}


void setWorkingMeth() {
    workTuples = &methTuples;
    workClauses = &methClauses;
    workInputTids = &methInputTids;
    workConsequentTids = &methConsequentTids;
    workTids = &methTids;
    workCids = &methCids;
    resultClauses.clear();
}


void populateFramework() {

    // Compute all tuples, all consequents and all input tuples.
    workTuples->clear();
    workConsequentTids->clear();
    workInputTids->clear();
    workTids->clear();
    workCids->clear();

    for (const auto& clause : *workClauses) {
        for (const auto& literal : clause) {
            workTuples->insert(lit2Tuple(literal));
        }
    }

    static size_t tid = 0; // Tuple id starts from 0
    for (const auto& tup : *workTuples) {
        if (tuple2Tid.find(tup) == tuple2Tid.end()) {
            tuple2Tid[tup] = tid;
            tid2Tuple[tid] = tup;
            tid++;
        }
        workTids->insert(tuple2Tid[tup]);
    }

    unordered_set<size_t> workConsequentTidsUnordered;
    for (const auto& clause : *workClauses) {
        size_t cid = clause2Cid[clause];
        size_t conTid = tuple2Tid[getConsequent(clause)];
        workCids->insert(cid);
        workConsequentTidsUnordered.insert(conTid);
        workConsequentTids->insert(conTid);
        cid2ConsequentTid[cid] = conTid;

        vector<string> antTuples = getAntecedents(clause);
        set<size_t> antTids;
        for (const auto& t : antTuples) {
            size_t antTid = tuple2Tid[t];
            antTids.insert(antTid);
            if (antecedentTid2Cids.find(antTid) == antecedentTid2Cids.end()) {
                antecedentTid2Cids[antTid] = set<size_t>(); 
            }
            antecedentTid2Cids[antTid].insert(cid);
        }
        cid2AntecedentTids[cid] = antTids;
        if (consequentTid2Cids.find(conTid) == consequentTid2Cids.end()) {
            consequentTid2Cids[conTid] = set<size_t>(); 
        }
        consequentTid2Cids[conTid].insert(cid);
    }

    for (const auto& tup : *workTuples) {
        size_t i = tuple2Tid[tup];
        if (workConsequentTidsUnordered.find(i) == workConsequentTidsUnordered.end()){
            workInputTids->insert(i);
        }
    }
    clog << "populateFramework:" << workTuples->size() << " tuples, "
         << workConsequentTids->size() << " consequents, "
         << workInputTids->size() << " input tuples." << endl;
}


void readInputProbsFromFile(string inRuleProbFileName) {
    // Read inRuleProbFileName and get the rule probabilities.
    ifstream inRuleProbFile(inRuleProbFileName);
    string fileLine;
    while (getline(inRuleProbFile, fileLine)) {
        istringstream inputRuleProbStream(fileLine);

        string ruleName;
        inputRuleProbStream >> ruleName;
        ruleName.pop_back();

        string probStr;
        inputRuleProbStream >> probStr; 
        unordered_set<size_t> cidSet = ruleName2Cids[ruleName];
        for (size_t cid : cidSet) cid2Prob[cid] = stod(probStr);
    }
    for (const auto& clause : *workClauses) {
        size_t cid = clause2Cid[clause];
        if (cid2Prob.find(cid) == cid2Prob.end())
            cid2Prob[cid] = defaultRuleProb;
    }   
    clog << "readInputProbsFromFile: Loaded in " << cid2Prob.size() << " rule probabilities." << endl;
    inRuleProbFile.close();
    return;
}


void readInputProbsFromMap(map<vector<string>, double>& clause2Prob) {
    for (auto clPbPair : clause2Prob) {
        vector<string> clause = clPbPair.first;
        double prob = clPbPair.second;
        if (workClauses->find(clause) != workClauses->end())
            cid2Prob[clause2Cid[clause]] = prob;
    }
    clog << "readInputProbsFromMap: Loaded in " << clause2Prob.size() << " rule probabilities." << endl;
    return;
}


void clearFramework() {
    allTuples.clear();
    allClauses.clear();
    allInputTids.clear();
    allConsequentTids.clear();
    allTids.clear();
    allCids.clear();

    methTuples.clear();
    methClauses.clear();
    methInputTids.clear();
    methConsequentTids.clear();
    methTids.clear();
    methCids.clear();

    clause2Cid.clear();
    cid2Clause.clear();
    tuple2Tid.clear();
    tid2Tuple.clear();
    cid2Prob.clear();
    cid2RuleName.clear();
    ruleName2Cids.clear();
    cid2ConsequentTid.clear();
    cid2AntecedentTids.clear();
    consequentTid2Cids.clear();

    resultClauses.clear();
    clog << "Cleared framework." << endl;
    return;
}


void initializeFramework (set<vector<string>>& setOfClauses) {
    clog << __LOGSTR_FULL__ << "DBG: initializeFramework1 (in memory) start" << endl;
    workClauses->clear();
    set<vector<string>> selfLoopClauses;
    size_t numSelfLoopClauses = 0;
    string ruleNamePrefix = "G";  // to create a "generated" rule name.
    for (vector<string> clause : setOfClauses) {
        assert(clause.size() >= 1);
        if (isSelfLoop(clause)) {
            numSelfLoopClauses++;
            selfLoopClauses.insert(clause);
            continue;
        }
        workClauses->insert(clause);
        if (clause2Cid.find(clause) == clause2Cid.end()) {
            allClauses.insert(clause);
            clause2Cid[clause] = cid;
            allCids.insert(cid);
            cid2Clause[cid] = clause;
            string ruleName = ruleNamePrefix + to_string(generatedNameNdx);
            generatedNameNdx++;
            cid2RuleName[cid] = ruleName;
            unordered_set<size_t> cidSet;
            cidSet.insert(cid);
            ruleName2Cids[ruleName] = cidSet;
            cid++;
        }
    }
    clog << "initializeFramework1 (in memory): Loaded " << workClauses->size() << " clauses." << endl;
    clog << "initializeFramework1 (in memory): Found " << to_string(numSelfLoopClauses) << " clauses with self loop." << endl;

    clog << __LOGSTR_FULL__ << "DBG: initializeFramework1 (in memory) read from args" << endl;
    populateFramework();
    resultClauses.clear();
    clog << __LOGSTR_FULL__ << "DBG: populate framework done" << endl;
}


void initializeFramework (set<vector<string>>& setOfClauses, map<vector<string>, double>& clause2Prob) {
    clog << __LOGSTR_FULL__ << "DBG: initializeFramework2 (in memory) start" << endl;
    workClauses->clear();
    set<vector<string>> selfLoopClauses;
    size_t numSelfLoopClauses = 0;
    string ruleNamePrefix = "G";  // to create a "generated" rule name.
    for (vector<string> clause : setOfClauses) {
        assert(clause.size() >= 1);
        if (isSelfLoop(clause)) {
            numSelfLoopClauses++;
            selfLoopClauses.insert(clause);
            continue;
        }
        workClauses->insert(clause);
        if (clause2Cid.find(clause) == clause2Cid.end()) {
            allClauses.insert(clause);
            clause2Cid[clause] = cid;
            allCids.insert(cid);
            cid2Clause[cid] = clause;
            string ruleName = ruleNamePrefix + to_string(generatedNameNdx);
            generatedNameNdx++;
            cid2RuleName[cid] = ruleName;
            unordered_set<size_t> cidSet;
            cidSet.insert(cid);
            ruleName2Cids[ruleName] = cidSet;
            cid++;
        }
    }
    clog << "initializeFramework2 (in memory): Loaded " << workClauses->size() << " clauses." << endl;
    clog << "initializeFramework2 (in memory): Found " << to_string(numSelfLoopClauses) << " clauses with self loop." << endl;

    clog << __LOGSTR_FULL__ << "DBG: initializeFramework2 (in memory) read from args" << endl;
    populateFramework();
    clog << __LOGSTR_FULL__ << "DBG: populate framework done" << endl;
    resultClauses.clear();
    readInputProbsFromMap(clause2Prob);
    clog << __LOGSTR_FULL__ << "DBG: read input probs done" << endl;
}


void initializeFramework (string inConsFileName, string inRuleProbFileName) {

    set<vector<string>> selfLoopClauses;
    workClauses->clear();
    size_t numSelfLoopClauses = 0;
    ifstream inConsFile(inConsFileName);
    string inputLine;
    while (getline(inConsFile, inputLine)) {
        istringstream inputLineStream(inputLine);

        string ruleName;
        inputLineStream >> ruleName;
        ruleName.pop_back();

        vector<string> clause;

        string token;
        bool lastNot = false;
        while (inputLineStream >> token) {
            if (token == "NOT") {
                lastNot = true;
            } else {
                if (token[token.size() - 1] == ',') {
                    token.pop_back();
                }
                clause.push_back(lastNot ? "NOT " + token : token);
                lastNot = false;
            }
        }

        assert(clause.size() >= 1);
        if (isSelfLoop(clause)) {
            numSelfLoopClauses++;
            selfLoopClauses.insert(clause);
            continue;
        }
        workClauses->insert(clause);
        if (clause2Cid.find(clause) == clause2Cid.end()) {
            allClauses.insert(clause);
            clause2Cid[clause] = cid;
            allCids.insert(cid);
            cid2Clause[cid] = clause;
            cid2RuleName[cid] = ruleName;
            if (ruleName2Cids.find(ruleName) == ruleName2Cids.end())
                ruleName2Cids[ruleName] = unordered_set<size_t>();
            ruleName2Cids[ruleName].insert(cid);
            cid++;
        }
    }
    inConsFile.close();
    clog << "initializeFramework (from files): Loaded " << workClauses->size() << " clauses." << endl;
    clog << "initializeFramework (from files): Found " << to_string(numSelfLoopClauses) << " clauses with self loop." << endl;

    populateFramework();
    resultClauses.clear();
    if (inRuleProbFileName != "") readInputProbsFromFile(inRuleProbFileName);
}


void eraseClause(vector<string> clause) {
    workClauses->erase(clause);
    size_t cid = clause2Cid.at(clause);
    workCids->erase(cid);
    string ruleName = cid2RuleName[cid];
    size_t conTid = cid2ConsequentTid[cid];
    set<size_t> antTids = cid2AntecedentTids[cid];
   
    // cid2Clause.erase(cid);
    // cid2Prob.erase(cid);
    // cid2RuleName.erase(cid);
    // cid2ConsequentTid.erase(cid);
    // cid2AntecedentTids.erase(cid);
    ruleName2Cids[ruleName].erase(cid);
    consequentTid2Cids[conTid].erase(cid);
    for (size_t antTid : antTids) antecedentTid2Cids[antTid].erase(cid);
    return;
}


void copyIpClausesToOp () {
    for (const auto& clause : *workClauses) {
        resultClauses.insert(clause);
    }
}


void emitFromFramework (string outConsFileName, string outRuleProbFileName) {
    ofstream outConsFile;
    outConsFile.open(outConsFileName);
    for (const auto& clause : resultClauses) {
        outConsFile << cid2RuleName.at(clause2Cid.at(clause)) << ": " << clause << endl;
    }
    outConsFile.close();

    if (outRuleProbFileName != "") {
        ofstream outRuleProbFile;
        outRuleProbFile.open(outRuleProbFileName);
        unordered_set<string> emittedRuleNames;
        for (const auto& clause : resultClauses) {
            size_t cid = clause2Cid.at(clause);
            string ruleName = cid2RuleName.at(cid);
            if (emittedRuleNames.find(ruleName) == emittedRuleNames.end()) {
                outRuleProbFile << ruleName << ": " << cid2Prob.at(cid) << endl;
                emittedRuleNames.insert(ruleName);
            }
        }   
        outRuleProbFile.close();
    }
    return;
}

// allConsequentTids == allResConsequentTids
bool sanityCheck1 () {
    bool sanityCheckPassed = true;
    set<size_t> allResConsequentTids;
    for (const auto& clause : resultClauses) {
        string resConsequent = getConsequent(clause);
        if (tuple2Tid.find(resConsequent) != tuple2Tid.end()) {
            allResConsequentTids.insert(tuple2Tid[resConsequent]);
        } else {
            sanityCheckPassed = false;
        }
    }

    clog << "Sanity Check1: " << to_string(workConsequentTids->size()) << "  " << to_string(allResConsequentTids.size()) << endl;
    if (allResConsequentTids != *workConsequentTids) sanityCheckPassed = false;
    clog << "Completed. Sanity Check1 passed: " << (sanityCheckPassed ? "true" : "false") << endl;
    return sanityCheckPassed;
}


// allConsequentTids contains allResConsequentTids
bool sanityCheck2 () {
    bool sanityCheckPassed = true;
    set<size_t> allResConsequentTids;
    for (const auto& clause : resultClauses) {
        string resConsequent = getConsequent(clause);
        if (tuple2Tid.find(resConsequent) != tuple2Tid.end()) {
            allResConsequentTids.insert(tuple2Tid[resConsequent]);
        } else {
            sanityCheckPassed = false;
        }
    }

    for (const auto& tid : allResConsequentTids) {
        if (workConsequentTids->find(tid) == workConsequentTids->end()) {
            sanityCheckPassed = false;
        }
    }
    clog << "Sanity Check2: " << to_string(workConsequentTids->size()) << "  " << to_string(allResConsequentTids.size()) << endl;
    clog << "Completed. Sanity Check2 passed: " << (sanityCheckPassed ? "true" : "false") << endl;
    return sanityCheckPassed;
}


// allConsequentTids is contained in allResConsequentTids
bool sanityCheck3 () {
    bool sanityCheckPassed = true;
    set<size_t> allResConsequentTids;
    for (const auto& clause : resultClauses) {
        string resConsequent = getConsequent(clause);
        if (tuple2Tid.find(resConsequent) != tuple2Tid.end()) {
            allResConsequentTids.insert(tuple2Tid[resConsequent]);
        } else {
            sanityCheckPassed = false;
        }
    }

    for (const auto& tid : *workConsequentTids) {
        if (allResConsequentTids.find(tid) == allResConsequentTids.end()) {
            sanityCheckPassed = false;
        }
    }
    clog << "Sanity Check3: " << to_string(workConsequentTids->size()) << "  " << to_string(allResConsequentTids.size()) << endl;
    clog << "Completed. Sanity Check3 passed: " << (sanityCheckPassed ? "true" : "false") << endl;
    return sanityCheckPassed;
}
