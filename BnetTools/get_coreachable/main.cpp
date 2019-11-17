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
#include "analysis.h"
#include "get_coreachable.h"


static set<string> readOpTuples (string opTupleFileName) {
    set<string> allOutputTuples;
    ifstream opTupleFile(opTupleFileName);
    string tup;
    while (opTupleFile >> tup) allOutputTuples.insert(tup);
    clog << __LOGSTR__ << "Loaded " << allOutputTuples.size() << " output tuples." << endl;
    opTupleFile.close();
    return allOutputTuples;
}


int main(int argc, char *argv[]) {

    if (argc < 5) {
        cerr << __LOGSTR__ << "Insufficient arguments!" << endl
                           << "./get_coreachable inConsFileName outConsFileName opTupleFileName [1/0] numLevels" << endl;
        return 1;
    }
    string inConsFileName = argv[1];
    string outConsFileName = argv[2];
    string opTupleFileName = argv[3];
    int sanityCheckOnly = stoi(argv[4]);
    int numLevels =stoi(argv[5]);
    string inRuleProbFileName = "";
    string outRuleProbFileName = "";

    setWorkingAll();
    initializeFramework(inConsFileName, inRuleProbFileName);
    set<string> allOutputTuples = readOpTuples(opTupleFileName);
    if (sanityCheckOnly == 0) {
        if (numLevels > 0) 
            getLevelBasedCoreachable(allOutputTuples, numLevels);
        else
            getCoreachable(allOutputTuples);
    } else {
        set<string> scTuples = getSanityCheckTuples();
        coreachSanityCheck(allOutputTuples, scTuples);
    }
    sanityCheck2();
    emitFromFramework(outConsFileName, outRuleProbFileName);
    return 0;
}
